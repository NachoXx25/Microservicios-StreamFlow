using EmailMicroservice.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddGrpc();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();
builder.Host.UseSerilog();
var app = builder.Build();

app.Run();
