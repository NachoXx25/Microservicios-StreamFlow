using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthMicroservice.Services;
using AuthMicroservice.src.Application.DTOs;
using AuthMicroservice.src.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            if(!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var result = await _authService.Login(loginDTO);
                await _monitoringEventService.PublishActionEventAsync(new ActionEvent
                {
                    ActionMessage = $"Usuario {result.Email} ha iniciado sesion",
                    Service = "AuthMicroservice"
                });
                return Ok(new { result });
            }
            catch (Exception ex)
            {
                await _monitoringEventService.PublishErrorEventAsync(new ErrorEvent
                {
                    ErrorMessage = $"Intento de inicio de sesion: {ex.Message}",
                    Service = "AuthMicroservice"
                });
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPatch("usuarios/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdatePassword(int id, UpdatePasswordDTO updatePasswordDTO)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                if(!User.Identity?.IsAuthenticated ?? true) return Unauthorized( new {error = "No autenticado"} );
                var jti = User.Claims.FirstOrDefault(x => x.Type == "Jti")?.Value ?? throw new ArgumentNullException("Jti no encontrado");
                var userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value ?? throw new ArgumentNullException("Id no encontrado");
                updatePasswordDTO.UserId = id.ToString();
                updatePasswordDTO.UserRequestId = userId;
                updatePasswordDTO.Jti = jti;
                var result = await _authService.ChangePassword(updatePasswordDTO);
                await _monitoringEventService.PublishActionEventAsync(new ActionEvent
                {
                    ActionMessage = $"Usuario {id} ha cambiado su contrasena",
                    Service = "AuthMicroservice"
                });
                return Ok(new { result });
            }
            catch (Exception ex)
            {
                await _monitoringEventService.PublishErrorEventAsync(new ErrorEvent
                {
                    ErrorMessage = $"Error al cambiar contrasena: {ex.Message}",
                    Service = "AuthMicroservice"
                });
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Cierra sesión y elimina el token JWT del usuario autenticado.
        /// </summary>
        /// <returns>Mensaje de éxito o error.</returns>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var jti = User.Claims.FirstOrDefault(x => x.Type == "Jti")?.Value ?? throw new ArgumentNullException("Jti no encontrado");
                var userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value ?? throw new ArgumentNullException("Id no encontrado");
                var result = await _authService.Logout(jti);
                await _monitoringEventService.PublishActionEventAsync(new ActionEvent
                {
                    ActionMessage = $"Usuario con JTI {userId} ha cerrado sesion",
                    Service = "AuthMicroservice"
                });
                return Ok(new { result });
            }
            catch (Exception ex)
            {
                await _monitoringEventService.PublishErrorEventAsync(new ErrorEvent
                {
                    ErrorMessage = $"Error al cerrar sesion: {ex.Message}",
                    Service = "AuthMicroservice"
                });
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}