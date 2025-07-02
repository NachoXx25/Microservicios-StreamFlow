using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
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

var connectionFactory = new ConnectionFactory();
connectionFactory.HostName = "rabbit_mq";
connectionFactory.UserName = "guest";
connectionFactory.Password = "guest";
connectionFactory.Port = 5672;
var connection = connectionFactory.CreateConnection();
builder.Services.AddHostedService<MonitoringEventConsumer>();
builder.Services.AddSingleton<RabbitMQService>();

builder.Services.AddDbContext<MonitoringContext>(options =>
{
    var mongoConnectionString = Env.GetString("MONGODB_CONNECTION");
    var databaseName = Env.GetString("MONGODB_DATABASE_NAME");

    MongoClient mongoClient = null;
    int maxRetryCount = 5;
    TimeSpan maxRetryDelay = TimeSpan.FromSeconds(60);
    int retryCount = 0;

    while (retryCount < maxRetryCount)
    {
        try
        {
            var mongoClientSettings = MongoClientSettings.FromConnectionString(mongoConnectionString);

            mongoClientSettings.RetryWrites = true;
            mongoClientSettings.RetryReads = true;
            mongoClientSettings.ConnectTimeout = TimeSpan.FromSeconds(30);
            mongoClientSettings.SocketTimeout = TimeSpan.FromSeconds(120);
            mongoClientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(30);
            mongoClientSettings.WaitQueueTimeout = TimeSpan.FromSeconds(30);
            mongoClientSettings.MaxConnectionPoolSize = 100;
            mongoClientSettings.MinConnectionPoolSize = 10;
            mongoClientSettings.HeartbeatInterval = TimeSpan.FromSeconds(10);

            mongoClient = new MongoClient(mongoClientSettings);

            var databases = mongoClient.ListDatabaseNames().ToList();
            Log.Information($"Conexión establecida después de {retryCount + 1} intento(s)");
            break;
        }
        catch (Exception ex) when (ex is MongoConnectionException || ex is TimeoutException || ex is System.Net.Sockets.SocketException)
        {
            retryCount++;

            if (retryCount >= maxRetryCount)
            {
                Log.Error($"Falló la conexión después de {maxRetryCount} reitentos");
                throw;
            }

            var delay = TimeSpan.FromSeconds(Math.Min(Math.Pow(2, retryCount), maxRetryDelay.TotalSeconds));
            Log.Error($"Intento de conexión {retryCount} fallida: {ex.Message}. Reitentando en {delay.TotalSeconds} segundos...");
            Thread.Sleep(delay);
        }
    }

    options.UseMongoDB(mongoClient, databaseName);
});

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

var app = builder.Build();

app.MapGrpcService<MonitoringGrpcService>();

app.Run();
