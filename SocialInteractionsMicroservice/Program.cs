using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using SocialInteractionsMicroservice.Services;
using SocialInteractionsMicroservice.src.Application.Services.Implements;
using SocialInteractionsMicroservice.src.Application.Services.Interfaces;
using SocialInteractionsMicroservice.src.Infrastructure.Data;
using SocialInteractionsMicroservice.src.Infrastructure.Repositories.Implements;
using SocialInteractionsMicroservice.src.Infrastructure.Repositories.Interfaces;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ISocialInteractionsRepository, SocialInteractionsRepository>();
builder.Services.AddScoped<IVideoRepository, VideoRepository>();
builder.Services.AddScoped<ISocialInteractionsService, SocialInteractionsService>();
builder.Services.AddGrpc();
builder.Services.AddDbContext<SocialInteractionsContext>(options => 
    options.UseMongoDB(Env.GetString("MONGODB_CONNECTION"),Env.GetString("MONGODB_DATABASE_NAME")));


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await DataSeeder.Initialize(scope.ServiceProvider);
}

app.MapGrpcService<SocialInteractionsGrpcService>();

app.Run();
