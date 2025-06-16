using System.Text;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PlaylistMicroservice.Services;
using PlaylistMicroservice.src.Application.Services.Implements;
using PlaylistMicroservice.src.Application.Services.Interfaces;
using PlaylistMicroservice.src.Infrastructure.Data;
using PlaylistMicroservice.src.Infrastructure.MessageBroker.Consumers;
using PlaylistMicroservice.src.Infrastructure.MessageBroker.Services;
using PlaylistMicroservice.src.Infrastructure.Repositories.Implements;
using PlaylistMicroservice.src.Infrastructure.Repositories.Interfaces;
using RabbitMQ.Client;
using Serilog;

Env.Load();
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi(); 
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddGrpc();
builder.Services.AddScoped<IPlaylistService, PlaylistService>();
builder.Services.AddScoped<IPlaylistRepository, PlaylistRepository>();
builder.Services.AddScoped<IVideoEventHandlerRepository, VideoEventHandlerRepository>();
builder.Services.AddScoped<IMonitoringEventService, MonitoringEventService>();
try
{
    var connectionFactory = new ConnectionFactory();
    connectionFactory.HostName = "localhost";
    connectionFactory.UserName = "guest";
    connectionFactory.Password = "guest";
    connectionFactory.Port = 5672;
    var connection = connectionFactory.CreateConnection();
    builder.Services.AddHostedService<VideoEventConsumer>();
    builder.Services.AddSingleton<RabbitMQService>();
}
catch (Exception ex)
{
    Log.Error("Error al realizar la conexión a RabbitMQ: {Message}", ex.Message);
}

// Configurar URLs explícitamente
builder.WebHost.ConfigureKestrel(options =>
{
    // Puerto para HTTP/REST API
    options.ListenLocalhost(5249, o => o.Protocols = HttpProtocols.Http1);

    // Puerto para gRPC (HTTP/2)
    options.ListenLocalhost(5250, o => o.Protocols = HttpProtocols.Http2);

});

//Conexión a base de datos de módulo de autenticación (PostgreSQL)
builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(
        Env.GetString("POSTGRES_CONNECTION_STRING"),
        npgsqlOptions => 
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5, 
                maxRetryDelay: TimeSpan.FromSeconds(60),
                errorCodesToAdd: null);
            npgsqlOptions.CommandTimeout(120);
        }
    ));

// Configurar Serilog
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
    try
    {
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        Log.Error("Error al aplicar migraciones a la base de datos: {Message}", ex.Message);
    }
}



app.MapOpenApi();
app.UseHttpsRedirection();
app.UseSwaggerUI();
app.UseSwagger();
app.MapControllers();
app.MapGrpcService<PlaylistGrpcService>();
app.Run();

