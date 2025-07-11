using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using AuthMicroservice.src.Infrastructure.Data;
using AuthMicroservice.src.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Builder;
using Serilog;
using AuthMicroservice.src.Application.Services.Interfaces;
using AuthMicroservice.src.Application.Services.Implements;
using AuthMicroservice.src.Infrastructure.Repositories.Interfaces;
using AuthMicroservice.src.Infrastructure.Repositories.Implements;
using AuthMicroservice.src.Infrastructure.MessageBroker.Consumers;
using AuthMicroservice.src.Infrastructure.MessageBroker.Services;
using RabbitMQ.Client;
using AuthMicroservice.Services;

Env.Load();

var builder = WebApplication.CreateBuilder(args);
static string GetRequiredEnvironmentVariable(string key)
{
    return Environment.GetEnvironmentVariable(key) ?? 
           Env.GetString(key) ?? 
           throw new ArgumentNullException($"{key} no encontrado");
}
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddIdentity<User, Role>().AddEntityFrameworkStores<DataContext>().AddDefaultTokenProviders();
builder.Services.AddScoped<IUserEventHandlerRepository, UserEventHandlerRepository>();
builder.Services.AddScoped<IMonitoringEventService, MonitoringEventService>();
builder.Services.AddHostedService<UserEventConsumer>();
builder.Services.AddSingleton<RabbitMQService>();

//Conexión a base de datos de módulo de autenticación (PostgreSQL)
builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(
        GetRequiredEnvironmentVariable("POSTGRES_CONNECTION_STRING"),
        npgsqlOptions => 
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5, 
                maxRetryDelay: TimeSpan.FromSeconds(60),
                errorCodesToAdd: null);
            npgsqlOptions.CommandTimeout(120);
        }
    ));

//Configuración de middleware de autenticación
builder.Services.AddAuthentication( options => {

    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer( options => {
    var jwtSecret = GetRequiredEnvironmentVariable("JWT_SECRET");
    options.TokenValidationParameters = new TokenValidationParameters (){
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateLifetime = true,
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero 
    };
});

//Configuración de identity
builder.Services.Configure<IdentityOptions>(options =>
{
    //Configuración de contraseña
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;

    //Configuración de Email
    options.User.RequireUniqueEmail = true;

    //Configuración de UserName 
    options.User.AllowedUserNameCharacters = "abcdefghijklmnñpqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._";

    //Configuración de retrys
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
});

//Alcance de servicios
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

//Alcance de repositorios
builder.Services.AddScoped<ITokenRepository, TokenRepository>();

// Configurar Serilog
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

var app = builder.Build();

//Llamado al dataseeder
using (var scope = app.Services.CreateScope())
{
    await DataSeeder.Initialize(scope.ServiceProvider);
}


app.MapOpenApi();
app.UseHttpsRedirection();
app.UseSwaggerUI();
app.UseSwagger();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();