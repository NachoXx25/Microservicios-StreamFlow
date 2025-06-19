using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AuthMicroservice.Services;
using AuthMicroservice.src.Application.DTOs;
using AuthMicroservice.src.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using UserMicroservice.src.Domain.Models;

namespace AuthMicroservice.src.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IMonitoringEventService _monitoringEventService;
        public AuthController(IAuthService authService, IMonitoringEventService monitoringEventService)
        {
            _authService = authService;
            _monitoringEventService = monitoringEventService;
        }

        /// <summary>
        /// Inicia sesión y genera un token JWT para el usuario autenticado.
        /// </summary>
        /// <param name="loginDTO">Credenciales del usuario.</param>
        /// <returns>Token JWT generado.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(loginDTO.Email)) throw new ArgumentNullException("Email es requerido");
                if (string.IsNullOrWhiteSpace(loginDTO.Password)) throw new ArgumentNullException("Password es requerido");
                var regex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
                if (!regex.IsMatch(loginDTO.Email)) throw new ArgumentException("El formato del email es invalido");
                var result = await _authService.Login(loginDTO);
                await _monitoringEventService.PublishActionEventAsync(new ActionEvent
                {
                    ActionMessage = $"Usuario {result.Email} ha iniciado sesion",
                    UserId = result.Id.ToString(),
                    UserEmail = result.Email,
                    UrlMethod = "POST/auth/login",
                    Service = "AuthMicroservice"
                });
                return Ok(result);
            }
            catch (Exception ex)
            {
                await _monitoringEventService.PublishErrorEventAsync(new ErrorEvent
                {
                    ErrorMessage = $"Intento de inicio de sesion: {ex.Message}",
                    Service = "AuthMicroservice"
                });
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("usuarios/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdatePassword(int id, UpdatePasswordDTO updatePasswordDTO)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(updatePasswordDTO.CurrentPassword)) throw new ArgumentNullException("La contraseña actual es requerida");
                if (string.IsNullOrWhiteSpace(updatePasswordDTO.NewPassword)) throw new ArgumentNullException("La nueva contraseña es requerida");
                if (string.IsNullOrWhiteSpace(updatePasswordDTO.ConfirmPassword)) throw new ArgumentNullException("La confirmación de la nueva contraseña es requerida");
                if (updatePasswordDTO.NewPassword != updatePasswordDTO.ConfirmPassword) throw new ArgumentException("Las contraseñas no coinciden");
                if (updatePasswordDTO.NewPassword.Length < 8 || updatePasswordDTO.NewPassword.Length > 20) throw new ArgumentException("La contraseña debe tener entre 8 y 20 caracteres");
                var regex = new Regex(@"^(?=.*[A-Z])(?=.*[0-9])(?=.*[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ])[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ0-9]+$");
                if (!regex.IsMatch(updatePasswordDTO.NewPassword)) throw new ArgumentException("La contraseña debe ser alfanumérica y contener al menos una mayúscula");
                if (!User.Identity?.IsAuthenticated ?? true) return Unauthorized(new { error = "No autenticado" });
                var jti = User.Claims.FirstOrDefault(x => x.Type == "Jti")?.Value ?? throw new ArgumentNullException("Jti no encontrado");
                var userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value ?? throw new ArgumentNullException("Id no encontrado");
                updatePasswordDTO.UserId = id.ToString();
                updatePasswordDTO.UserRequestId = userId;
                updatePasswordDTO.Jti = jti;
                var result = await _authService.ChangePassword(updatePasswordDTO);
                await _monitoringEventService.PublishActionEventAsync(new ActionEvent
                {
                    ActionMessage = $"Usuario {id} ha cambiado su contrasena",
                    UserId = userId,
                    UserEmail = User.Claims.FirstOrDefault(x => x.Type == "Email")?.Value ?? "",
                    UrlMethod = $"PATCH/auth/usuarios/{id}",
                    Service = "AuthMicroservice"
                });
                return Ok(result);
            }
            catch (Exception ex)
            {
                await _monitoringEventService.PublishErrorEventAsync(new ErrorEvent
                {
                    ErrorMessage = $"Error al cambiar contrasena: {ex.Message}",
                    UserId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value ?? "",
                    UserEmail = User.Claims.FirstOrDefault(x => x.Type == "Email")?.Value ?? "",
                    Service = "AuthMicroservice"
                });
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Cierra sesión y elimina el token JWT del usuario autenticado.
        /// </summary>
        /// <returns>Mensaje de éxito o error.</returns>
        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            try
            {
                if (!User.Identity?.IsAuthenticated ?? true) return Unauthorized("No autenticado");
                var jti = User.Claims.FirstOrDefault(x => x.Type == "Jti")?.Value ?? throw new ArgumentNullException("Error en el sistema: Jti no encontrado");
                var userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value ?? throw new ArgumentNullException("Error en el sistema: Id no encontrado");
                var result = await _authService.Logout(jti);
                await _monitoringEventService.PublishActionEventAsync(new ActionEvent
                {
                    ActionMessage = $"Usuario con JTI {userId} ha cerrado sesion",
                    UserId = userId,
                    UserEmail = User.Claims.FirstOrDefault(x => x.Type == "Email")?.Value ?? "",
                    UrlMethod = "POST/auth/logout",
                    Service = "AuthMicroservice"
                });
                return Ok(result);
            }
            catch (Exception ex)
            {
                await _monitoringEventService.PublishErrorEventAsync(new ErrorEvent
                {
                    ErrorMessage = $"Error al cerrar sesion: {ex.Message}",
                    UserId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value ?? "",
                    UserEmail = User.Claims.FirstOrDefault(x => x.Type == "Email")?.Value ?? "",
                    Service = "AuthMicroservice"
                });
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("validate-token")]
        public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequestDTO request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Token))
                {
                    await _monitoringEventService.PublishErrorEventAsync(new ErrorEvent
                    {
                        ErrorMessage = "El token es requerido para la validación",
                        UserId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value ?? "",
                        UserEmail = User.Claims.FirstOrDefault(x => x.Type == "Email")?.Value ?? "",
                        Service = "AuthMicroservice"
                    });
                    return BadRequest(new { message = "El token es requerido" });
                }
                var isBlacklisted = await _authService.IsTokenBlacklistedAsync(request.Token);

                if (isBlacklisted)
                {
                    await _monitoringEventService.PublishErrorEventAsync(new ErrorEvent
                    {
                        UserId = request.UserId ?? User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value ?? "",
                        UserEmail = request.UserEmail ?? User.Claims.FirstOrDefault(x => x.Type == "Email")?.Value ?? "",
                        Service = "AuthMicroservice"
                    });
                    return Ok(new TokenValidationResponseDTO
                    {
                        IsBlacklisted = true,
                        Message = "Token inválido"
                    });
                }
                await _monitoringEventService.PublishActionEventAsync(new ActionEvent
                {
                    ActionMessage = "Token validado correctamente",
                    UserId = request.UserId ?? User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value ?? "",
                    UserEmail = request.UserEmail ?? User.Claims.FirstOrDefault(x => x.Type == "Email")?.Value ?? "",
                    UrlMethod = "POST/auth/validate-token",
                    Service = "AuthMicroservice"
                });
                return Ok(new TokenValidationResponseDTO
                {
                    IsBlacklisted = false,
                    Message = "Token válido"
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error validating token");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult CheckHealth()
        {
            try
            {
                Log.Information("Recibida petición para verificar la salud del servicio");
                return Ok(new { IsRunning = true });
            }
            catch (Exception ex)
            {
                Log.Error($"Error al verificar la salud del servicio: {ex.Message}");
                return StatusCode(500, new { message = "Error al verificar la salud del servicio" });
            }
        }
    }
}