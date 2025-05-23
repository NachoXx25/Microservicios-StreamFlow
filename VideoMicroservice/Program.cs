using DotNetEnv;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using VideoMicroservice.Protos;
using VideoMicroservice.Services;
using VideoMicroservice.src.Application.Services.Implements;
using VideoMicroservice.src.Application.Services.Interfaces;
using VideoMicroservice.src.Infrastructure.Data;
using VideoMicroservice.src.Infrastructure.Repositories.Implements;
using VideoMicroservice.src.Infrastructure.Repositories.Interfaces;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IVideoEventService, VideoEventService>();
builder.Services.AddScoped<IVideoService,VideoService>();
builder.Services.AddScoped<IVideoRepository, VideoRepository>();
builder.Services.AddGrpc();
builder.Services.AddDbContext<VideoContext>(options => 
    options.UseMongoDB(Env.GetString("MONGODB_CONNECTION"),Env.GetString("MONGODB_DATABASE_NAME")));


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await DataSeeder.Initialize(scope.ServiceProvider);
}


app.MapGrpcService<VideoMicroservice.Services.VideoGrpcService>();
app.Run();
