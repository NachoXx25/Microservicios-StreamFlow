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
builder.Services.AddScoped<SocialInteractionsEventService>();

try
{
    var connectionFactory = new ConnectionFactory();
    connectionFactory.HostName = Env.GetString("RABBITMQ_HOST") ?? "localhost";
    connectionFactory.UserName = Env.GetString("RABBITMQ_USERNAME") ?? "guest";
    connectionFactory.Password = Env.GetString("RABBITMQ_PASSWORD") ?? "guest";
    connectionFactory.Port = Env.GetInt("RABBITMQ_PORT");
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

using (var scope = app.Services.CreateScope())
{
    await DataSeeder.Initialize(scope.ServiceProvider);
}

app.MapGrpcService<SocialInteractionsGrpcService>();

app.Run();
