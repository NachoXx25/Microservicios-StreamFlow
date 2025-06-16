using DotNetEnv;
using EmailMicroservice.src.Implements;
using EmailMicroservice.src.Infrastructure.MessageBroker.Consumers;
using EmailMicroservice.src.Infrastructure.MessageBroker.Services;
using EmailMicroservice.src.Interfaces;
using Serilog;

Env.Load();

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddGrpc();

builder.Services.AddScoped<IBillEventHandler, BillEventHandler>();
builder.Services.AddHostedService<BillEventConsumer>();
builder.Services.AddSingleton<RabbitMQService>();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();
builder.Host.UseSerilog();
var app = builder.Build();

app.Run();
