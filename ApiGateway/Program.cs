using System.Text;
using ApiGateway.Middleware;
using ApiGateway.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;

Env.Load();
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddGrpc();
builder.Services.AddSingleton<UserGrpcClient>();
builder.Services.AddSingleton<PlaylistGrpcClient>();
builder.Services.AddSingleton<AuthHttpClient>();
builder.Services.AddSingleton<MonitoringGrpcClient>();
builder.Services.AddSingleton<BillGrpcClient>();
builder.Services.AddSingleton<VideoGrpcClient>();
builder.Services.AddSingleton<SocialInteractionsGrpcClient>();
builder.Services.AddHttpClient();
//Configuración de middleware de autenticación
builder.Services.AddAuthentication(options =>
{

    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{

    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Env.GetString("JWT_SECRET"))),
        ValidateLifetime = true,
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

// Configurar Serilog
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

var app = builder.Build();

app.MapOpenApi();
app.UseHttpsRedirection();
app.UseSwaggerUI();
app.UseSwagger();
app.UseAuthentication();
app.UseMiddleware<BlacklistValidationMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.Run();
