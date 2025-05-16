using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthMicroservice.src.Application.DTOs;
using AuthMicroservice.src.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthMicroservice.src.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
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
                return Ok( new {result} );
            }
            catch (Exception ex)
            {
                return BadRequest( new {error = ex.Message} );
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
                return Ok( new {result} );
            }
            catch (Exception ex)
            {
                return BadRequest( new {error = ex.Message} );
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
                var result = await _authService.Logout(jti);
                return Ok( new {result} );
            }
            catch (Exception ex)
            {
                return BadRequest( new {error = ex.Message} );
            }
        }
    }
}