using BillMicroservice.Services;
using BillMicroservice.src.Application.Services.Implements;
using BillMicroservice.src.Application.Services.Interfaces;
using BillMicroservice.src.Domain.Models.User;
using BillMicroservice.src.Infrastructure.Data;
using BillMicroservice.src.Infrastructure.Repositories.Implements;
using BillMicroservice.src.Infrastructure.Repositories.Interfaces;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Serilog;
using RabbitMQ.Client;
using BillMicroservice.src.Infrastructure.MessageBroker.Consumers;
using BillMicroservice.src.Infrastructure.MessageBroker.Services;

Env.Load();
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentity<User, Role>().AddEntityFrameworkStores<BillContext>().AddDefaultTokenProviders();

builder.Services.AddScoped<IBillRepository, BillRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IStatusRepository, StatusRepository>();
builder.Services.AddScoped<IUserEventHandlerRepository, UserEventHandlerRepository>();
builder.Services.AddScoped<IBillEventService, BillEventService>();
builder.Services.AddScoped<IBillService, BillService>();
builder.Services.AddScoped<IMonitoringEventService, MonitoringEventService>();

try
{
    var connectionFactory = new ConnectionFactory();
    connectionFactory.HostName = "rabbit_mq";
    connectionFactory.UserName = "guest";
    connectionFactory.Password = "guest";
    connectionFactory.Port = 5672;
    var connection = connectionFactory.CreateConnection();
    builder.Services.AddHostedService<UserEventConsumer>();
    builder.Services.AddSingleton<RabbitMQService>();
}
catch (Exception ex)
{
    Log.Error("Error al realizar la conexión a RabbitMQ: {Message}", ex.Message);
}

builder.Services.AddGrpc();

var serverVersion = new MySqlServerVersion(new Version(8, 0, 21));
builder.Services.AddDbContext<BillContext>(options =>
    options.UseMySql(Env.GetString("MARIADB_CONNECTION"), serverVersion,
        mySqlOptions =>
        {
            mySqlOptions.MigrationsAssembly(typeof(BillContext).Assembly.FullName);
            mySqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(60),
                errorNumbersToAdd: null
            );
            mySqlOptions.CommandTimeout(120);
        }));

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await DataSeeder.Initialize(scope.ServiceProvider);
}

app.MapGrpcService<BillGrpcService>();

app.Run();
