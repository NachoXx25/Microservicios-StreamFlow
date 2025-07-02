using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using RabbitMQ.Client;
using Serilog;
using SocialInteractionsMicroservice.Services;
using SocialInteractionsMicroservice.src.Application.Services.Implements;
using SocialInteractionsMicroservice.src.Application.Services.Interfaces;
using SocialInteractionsMicroservice.src.Infrastructure.Data;
using SocialInteractionsMicroservice.src.Infrastructure.MessageBroker.Consumers;
using SocialInteractionsMicroservice.src.Infrastructure.MessageBroker.Services;
using SocialInteractionsMicroservice.src.Infrastructure.Repositories.Implements;
using SocialInteractionsMicroservice.src.Infrastructure.Repositories.Interfaces;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ILikeRepository, LikeRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IVideoRepository, VideoRepository>();
builder.Services.AddScoped<IVideoEventHandlerRepository, VideoEventHandlerRepository>();
builder.Services.AddScoped<ISocialInteractionsService, SocialInteractionsService>();
builder.Services.AddScoped<ISocialInteractionsEventService, SocialInteractionsEventService>();
builder.Services.AddScoped<IMonitoringEventService, MonitoringEventService>();

var connectionFactory = new ConnectionFactory();
connectionFactory.HostName = "rabbit_mq";
connectionFactory.UserName = "guest";
connectionFactory.Password = "guest";
connectionFactory.Port = 5672;
var connection = connectionFactory.CreateConnection();
builder.Services.AddHostedService<VideoEventConsumer>();
builder.Services.AddSingleton<RabbitMQService>();

builder.Services.AddGrpc();
builder.Services.AddDbContext<SocialInteractionsContext>(options =>
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

app.MapGrpcService<SocialInteractionsGrpcService>();

app.Run();
