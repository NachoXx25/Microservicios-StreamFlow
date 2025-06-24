using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ApiGateway.src.Application.DTOs;
using DotNetEnv;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace ApiGateway.Services
{
    public class AuthHttpClient : IDisposable
    {

        private readonly string _baseUrl;
        private readonly HttpClient _client;

        public AuthHttpClient(HttpClient client, IConfiguration configuration)
        {
            _baseUrl = Env.GetString("Services__AuthService") ?? "http://localhost:5184/";
            _client = client;
            _client.BaseAddress = new Uri(_baseUrl);
            _client.DefaultRequestHeaders.Add("Accept", "application/json");
            _client.Timeout = TimeSpan.FromSeconds(30);
            Log.Information("Iniciando AuthHttpClient con base URL: {BaseUrl}", _baseUrl);
        }

        public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request)
        {
            try
            {
                Log.Information("Intentando login para: {Email}", request.Email);

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json")
                );

                var response = await _client.PostAsync("/auth/login", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var authResponse = JsonSerializer.Deserialize<LoginResponseDTO>(responseContent, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    Log.Information("✅ Login exitoso para: {Email}", request.Email);
                    return authResponse ?? throw new InvalidOperationException("Error en el servidor");
                }
                else
                {
                    var errorMessage = "Error de autenticación desconocido";
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Log.Warning("Login fallido para {Email}: {Error}", request.Email, errorContent);
                    if(errorContent.StartsWith("\"") && errorContent.EndsWith("\""))
                    {
                        errorMessage = JsonSerializer.Deserialize<string>(errorContent) ?? errorMessage;
                    }
                    else
                    {
                        errorMessage = errorContent;
                    }
                    throw new UnauthorizedAccessException(errorMessage);
                }
            }
            catch (HttpRequestException ex)
            {
                Log.Error(ex, "Error de red en login para: {Email}", request.Email);
                throw new InvalidOperationException("AuthService no disponible", ex);
            }
        }

        public async Task<string> ChangePasswordAsync(string? id, UpdatePasswordDTO request, string? token)
        {
            try
            {
                Log.Information("Intentando cambiar contraseña para el usuario: {UserId}", id);
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json")
                );
                using var requestMessage = new HttpRequestMessage(HttpMethod.Patch, $"/auth/usuarios/{id}")
                {
                    Content = jsonContent
                };

                if (!string.IsNullOrEmpty(token))
                {
                    requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    Log.Information("🔑 Token JWT agregado al request de cambio de contraseña");
                }
                var response = await _client.SendAsync(requestMessage);

                if (response.IsSuccessStatusCode)
                {
                    Log.Information("✅ Contraseña cambiada exitosamente para el usuario: {UserId}", id);
                    return "Contraseña cambiada exitosamente";
                }
                else
                {
                    var errorMessage = "Error al cambiar la contraseña";
                    var errorContent = await response.Content.ReadAsStringAsync();
                    if(errorContent.StartsWith("\"") && errorContent.EndsWith("\""))
                    {
                        errorMessage = JsonSerializer.Deserialize<string>(errorContent) ?? errorMessage;
                    }
                    else
                    {
                        errorMessage = errorContent;
                    }
                    Log.Warning("Cambio de contraseña fallido para el usuario: {UserId}: {Error}", id, errorContent);
                    throw new InvalidOperationException($"Error al cambiar la contraseña: {errorMessage}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException("AuthService no disponible", ex);
            }
        }

        public async Task<string> LogoutAsync(string? jti, string? token)
        {
            try
            {
                Log.Information("Intentando cerrar sesión con JTI: {Jti}", jti);
                using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/auth/logout");

                if (!string.IsNullOrEmpty(token))
                {
                    requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    Log.Information("🔑 Token JWT agregado al request de logout");
                }
                var response = await _client.SendAsync(requestMessage);

                if (response.IsSuccessStatusCode)
                {
                    Log.Information("✅ Cierre de sesión exitoso para JTI: {Jti}", jti);
                    return "Cierre de sesión exitoso";
                }
                else
                {
                    var errorMessage = "Error al cerrar sesión";
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Log.Warning("Cierre de sesión fallido para JTI: {Jti}: {Error}", jti, errorContent);
                    if(errorContent.StartsWith("\"") && errorContent.EndsWith("\""))
                    {
                        errorMessage = JsonSerializer.Deserialize<string>(errorContent) ?? errorMessage;
                    }
                    else
                    {
                        errorMessage = errorContent;
                    }
                    throw new InvalidOperationException($"Error al cerrar sesión: {errorMessage}");
                }
            }
            catch (HttpRequestException ex)
            {
                Log.Error(ex, "Error de red al intentar cerrar sesión con JTI: {Jti}", jti);
                throw new InvalidOperationException("AuthService no disponible", ex);
            }
        }
    
        public void Dispose()
        {
            _client.Dispose();
        }
    }
}