using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthMicroservice.src.Application.DTOs;
using AuthMicroservice.src.Application.Services.Interfaces;
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
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}