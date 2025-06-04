using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiGateway.Services;
using ApiGateway.src.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.src.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthHttpClient _authHttpClient;

        public AuthController(AuthHttpClient authHttpClient)
        {
            _authHttpClient = authHttpClient;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDTO request)
        {
            try
            {
                var response = await _authHttpClient.LoginAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPatch("usuarios/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> ChangePassword(string id, UpdatePasswordDTO request)
        {
            try
            {
                var userId = User?.FindFirst("Id")?.Value;
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                string? token = null;
                
                if (authHeader?.StartsWith("Bearer ") == true)
                {
                    token = authHeader.Substring("Bearer ".Length).Trim();
                }
                var response = await _authHttpClient.ChangePasswordAsync(id, request, token);
                return Ok(new { message = response });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var jti = User?.FindFirst("jti")?.Value;
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                string? token = null;
                
                if (authHeader?.StartsWith("Bearer ") == true)
                {
                    token = authHeader.Substring("Bearer ".Length).Trim();
                }
                var response = await _authHttpClient.LogoutAsync(jti, token);
                return Ok(new { message = response });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}