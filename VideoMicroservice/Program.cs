using DotNetEnv;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using Serilog;
using VideoMicroservice.Protos;
using VideoMicroservice.Services;
using VideoMicroservice.src.Application.Services.Implements;
using VideoMicroservice.src.Application.Services.Interfaces;
using VideoMicroservice.src.Infrastructure.Data;
using VideoMicroservice.src.Infrastructure.MessageBroker.Consumers;
using VideoMicroservice.src.Infrastructure.MessageBroker.Services;
using VideoMicroservice.src.Infrastructure.Repositories.Implements;
using VideoMicroservice.src.Infrastructure.Repositories.Interfaces;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IVideoEventService, VideoEventService>();
builder.Services.AddScoped<IVideoService, VideoService>();
builder.Services.AddScoped<IMonitoringEventService, MonitoringEventService>();
builder.Services.AddScoped<IVideoRepository, VideoRepository>();
builder.Services.AddScoped<ISocialInteractionEventHandlerRepository, SocialInteractionEventHandlerRepository>();
builder.Services.AddGrpc();

try
{
    var connectionFactory = new ConnectionFactory();
    connectionFactory.HostName = "rabbit_mq";
    connectionFactory.UserName = "guest";
    connectionFactory.Password = "guest";
    connectionFactory.Port = 5672;
    var connection = connectionFactory.CreateConnection();
    builder.Services.AddHostedService<SocialInteractionEventConsumer>();
    builder.Services.AddSingleton<RabbitMQService>();
}
catch (Exception ex)
{
    Log.Error("Error al realizar la conexión a RabbitMQ: {Message}", ex.Message);
}

builder.Services.AddDbContext<VideoContext>(options =>
    options.UseMongoDB(Env.GetString("MONGODB_CONNECTION"), Env.GetString("MONGODB_DATABASE_NAME")));

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await DataSeeder.Initialize(scope.ServiceProvider);
}

app.MapGrpcService<VideoMicroservice.Services.VideoGrpcService>();
app.Run();
