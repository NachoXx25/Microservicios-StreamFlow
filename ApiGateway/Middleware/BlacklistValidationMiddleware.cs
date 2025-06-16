using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Serilog;

namespace ApiGateway.Middleware
{
    public class BlacklistValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _authServiceUrl;

        public BlacklistValidationMiddleware(
            RequestDelegate next,
            IHttpClientFactory httpClientFactory,
            ILogger<BlacklistValidationMiddleware> logger,
            IConfiguration configuration)
        {
            _next = next;
            _httpClientFactory = httpClientFactory;

            _authServiceUrl = configuration["Services:AuthService"] ?? "http://localhost:5184";
        }

        public async Task InvokeAsync(HttpContext context)
        {
           
            var token = ExtractTokenFromRequest(context);

            if (!string.IsNullOrEmpty(token))
            {
                Log.Information("Validando token en blacklist...");

                try
                {
                    var isBlacklisted = await IsTokenBlacklistedAsync(token);

                    if (isBlacklisted)
                    {
                        Log.Warning("Token está en blacklist - Acceso denegado");

                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";

                        var response = new
                        {
                            error = "Token inválido",
                            message = "Tu sesión ha sido invalidada. Por favor, inicia sesión nuevamente."
                        };

                        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                        return; 
                    }

                    Log.Debug("✅ Token válido - continuando");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error validando token en blacklist");
                    Log.Warning("Continuando debido a error en validación de blacklist");
                }
            }

            await _next(context);
        }

        private string? ExtractTokenFromRequest(HttpContext context)
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            if (authHeader?.StartsWith("Bearer ") == true)
            {
                return authHeader.Substring("Bearer ".Length).Trim();
            }

            return null;
        }

        private async Task<bool> IsTokenBlacklistedAsync(string token)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(5); 

                var checkUrl = $"{_authServiceUrl}auth/validate-token";

                var requestBody = new
                {
                    token = token
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                Log.Debug("📡 Consultando AuthService: {Url}", checkUrl);

                var response = await httpClient.PostAsync(checkUrl, jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<TokenValidationResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return result?.IsBlacklisted == true;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return false;
                }
                else
                {
                    Log.Warning("AuthService respondió con status: {StatusCode}", response.StatusCode);
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                Log.Error(ex, "Error de red consultando AuthService");
                return false;
            }
            catch (TaskCanceledException ex)
            {
                Log.Error(ex, "Timeout consultando AuthService");
                return false; 
            }
        }
        public class TokenValidationResponse
        {
            public bool IsBlacklisted { get; set; }
            public string? Message { get; set; }
            public DateTime? BlacklistedAt { get; set; }
        }
    }
}