using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using MonitoringMicroservice.Services;
using MonitoringMicroservice.src.Application.Services.Implements;
using MonitoringMicroservice.src.Application.Services.Interfaces;
using MonitoringMicroservice.src.Infrastructure.Data;
using MonitoringMicroservice.src.Infrastructure.MessageBroker.Consumers;
using MonitoringMicroservice.src.Infrastructure.MessageBroker.Services;
using MonitoringMicroservice.src.Infrastructure.Repositories.Implements;
using MonitoringMicroservice.src.Infrastructure.Repositories.Interfaces;
using RabbitMQ.Client;
using Serilog;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IActionRepository, ActionRepository>();
builder.Services.AddScoped<IErrorRepository, ErrorRepository>();
builder.Services.AddScoped<IMonitoringEventHandler, MonitoringEventHandler>();
builder.Services.AddScoped<IMonitoringService, MonitoringService>();
builder.Services.AddGrpc();

try
{
    var connectionFactory = new ConnectionFactory();
    connectionFactory.HostName = Env.GetString("RABBITMQ_HOST") ?? "localhost";
    connectionFactory.UserName = Env.GetString("RABBITMQ_USERNAME") ?? "guest";
    connectionFactory.Password = Env.GetString("RABBITMQ_PASSWORD") ?? "guest";
    connectionFactory.Port = Env.GetInt("RABBITMQ_PORT");
    var connection = connectionFactory.CreateConnection();
    builder.Services.AddHostedService<MonitoringEventConsumer>();
    builder.Services.AddSingleton<RabbitMQService>();
}catch (Exception ex)
{
    Log.Error("Error al realizar la conexión a RabbitMQ: {Message}", ex.Message);
}

builder.Services.AddDbContext<MonitoringContext>(options => 
    options.UseMongoDB(Env.GetString("MONGODB_CONNECTION"),Env.GetString("MONGODB_DATABASE_NAME")));

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

var app = builder.Build();

app.MapGrpcService<MonitoringGrpcService>();

app.Run();
