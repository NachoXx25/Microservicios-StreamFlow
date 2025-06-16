using DotNetEnv;
using Microsoft.EntityFrameworkCore;
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
builder.Services.AddScoped<IVideoEventHandlerRepository,VideoEventHandlerRepository>();
builder.Services.AddScoped<ISocialInteractionsService, SocialInteractionsService>();
builder.Services.AddScoped<ISocialInteractionsEventService, SocialInteractionsEventService>();
builder.Services.AddScoped<IMonitoringEventService, MonitoringEventService>();

try
{
    var connectionFactory = new ConnectionFactory();
    connectionFactory.HostName = "localhost";
    connectionFactory.UserName = "guest";
    connectionFactory.Password ="guest";
    connectionFactory.Port = 5672;
    var connection = connectionFactory.CreateConnection();
    builder.Services.AddHostedService<VideoEventConsumer>();
    builder.Services.AddSingleton<RabbitMQService>();
}
catch (Exception ex)
{
    Log.Error("Error al realizar la conexión a RabbitMQ: {Message}", ex.Message);
}

builder.Services.AddGrpc();
builder.Services.AddDbContext<SocialInteractionsContext>(options => 
    options.UseMongoDB(Env.GetString("MONGODB_CONNECTION"),Env.GetString("MONGODB_DATABASE_NAME")));


builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

var app = builder.Build();

app.MapGrpcService<SocialInteractionsGrpcService>();

app.Run();
