using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using MonitoringMicroservice.Services;
using MonitoringMicroservice.src.Application.Services.Implements;
using MonitoringMicroservice.src.Application.Services.Interfaces;
using MonitoringMicroservice.src.Infrastructure.Data;
using MonitoringMicroservice.src.Infrastructure.Repositories.Implements;
using MonitoringMicroservice.src.Infrastructure.Repositories.Interfaces;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IActionRepository, ActionRepository>();
builder.Services.AddScoped<IErrorRepository, ErrorRepository>();
builder.Services.AddScoped<IMonitoringService, MonitoringService>();
builder.Services.AddGrpc();

builder.Services.AddDbContext<MonitoringContext>(options => 
    options.UseMongoDB(Env.GetString("MONGODB_CONNECTION"),Env.GetString("MONGODB_DATABASE_NAME")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await DataSeeder.Initialize(scope.ServiceProvider);
}

app.MapGrpcService<MonitoringGrpcService>();

app.Run();
