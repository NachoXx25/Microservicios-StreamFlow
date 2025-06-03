using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiGateway.Protos.UserService;
using ApiGateway.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.src.Api.Controllers
{
    [ApiController]
    [Route("")]
    public class UserController : ControllerBase
    {
        private readonly UserGrpcClient _userGrpcClient;

        public UserController(UserGrpcClient userGrpcClient)
        {
            _userGrpcClient = userGrpcClient;
        }

        [HttpGet("usuarios")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                if (!User.Identity?.IsAuthenticated == true)
                {
                    return Unauthorized(new { Error = "Se requiere autenticación para acceder a esta información." });
                }
                if(!User.IsInRole("Administrador"))
                {
                    throw new Exception("No tienes permisos para acceder a esta información.");
                }
                var response = await _userGrpcClient.GetAllUsersAsync();
                return Ok(response.Users);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }

        }

        [HttpGet("usuarios/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserById(string id)
        {
            try{
                if(!User.Identity?.IsAuthenticated == true)
                {
                    return Unauthorized(new { Error = "Se requiere autenticación para acceder a esta información." });
                }
                var userIdClaim = User.FindFirst("Id")?.Value;
                if(userIdClaim != id.ToString() && !User.IsInRole("Administrador")) throw new Exception("No puedes acceder a otros usuarios.");
                var response = await _userGrpcClient.GetUserByIdAsync(id);
            return Ok(response.User);
            }catch(Exception ex)
            {
                return NotFound(new { Error = ex.Message});
            }
        }

        [HttpPost("usuarios")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                if (request.Role?.ToLower() == "administrador")
                {
                    if (!User.Identity?.IsAuthenticated == true)
                    {
                        return Unauthorized(new { Error = "Se requiere autenticación para crear usuarios administradores." });
                    }
                    
                    if (!User.IsInRole("Administrador"))
                    {
                        throw new Exception("No tienes permisos para crear usuarios administradores.");
                    }
                }       
                var response = await _userGrpcClient.CreateUserAsync(request);
                return CreatedAtAction(nameof(GetUserById), new { id = response.Id }, response);
            }catch(Exception ex)
            {
                return BadRequest( new { Error = ex.Message});
            }
        }

        [HttpPatch("usuarios/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                if (!User.Identity?.IsAuthenticated == true)
                {
                    return Unauthorized(new { Error = "Se requiere autenticación." });
                }
                var userIdClaim = User.FindFirst("Id")?.Value;
                if (userIdClaim != id.ToString() && !User.IsInRole("Administrador")) throw new Exception("No puedes editar a otros usuarios.");
                if (!string.IsNullOrEmpty(request.Password)) throw new Exception("No puedes editar la contraseña campo aquí.");
                request.Id = id;
                var response = await _userGrpcClient.UpdateUserAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpDelete("usuarios/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                if (!User.Identity?.IsAuthenticated == true)
                {
                    return Unauthorized(new { Error = "Se requiere autenticación." });
                }
                var userIdClaim = User.FindFirst("Id")?.Value;
                if (userIdClaim == id.ToString())
                {
                    throw new Exception("No puedes eliminarte a ti mismo.");
                }
                if (!User.IsInRole("Administrador"))
                {
                    throw new Exception("No tienes permisos para eliminar usuarios.");
                }
                await _userGrpcClient.DeleteUserAsync(new DeleteUserRequest { Id = id });
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}