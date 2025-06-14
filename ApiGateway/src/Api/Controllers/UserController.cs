using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ApiGateway.Protos.UserService;
using ApiGateway.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

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
        [AllowAnonymous]
        public async Task<IActionResult> GetAllUsers([FromForm] GetAllUsersRequest request)
        {
            try
            {
                request.UserId = User.FindFirst("Id")?.Value ?? "";
                request.UserEmail = User.FindFirst("Email")?.Value ?? "";
                request.Role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
                var response = await _userGrpcClient.GetAllUsersAsync(request);
                var users = response.Users.Select(u => new
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    CreatedAt = u.CreatedAt?.ToDateTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                }).ToList();
                
                return Ok(users);
            }
            catch (Exception ex)
            {
                if(ex.Message.ToLower().Contains("no autenticado"))
                {
                    return Unauthorized(new { error = ex.Message });
                }
                if(ex.Message.ToLower().Contains("error en el sistema"))
                {
                    return StatusCode(500, new { error = "Error en el sistema, intente más tarde" });
                }
                if(ex.Message.ToLower().Contains("no encontrado"))
                {
                    return NotFound(new { error = ex.Message });
                }
                if(ex.Message.ToLower().Contains("no tienes permisos"))
                {
                    return StatusCode(403, new { error = "No tienes permisos para realizar esta acción" });
                }
                return BadRequest(new { error = ex.Message });
            }

        }

        [HttpGet("usuarios/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserById(string id)
        {
            try{
                var request = new GetUserByIdRequest
                {
                    Id = id,
                    UserId = User.FindFirst("Id")?.Value ?? "",
                    UserEmail = User.FindFirst("Email")?.Value ?? "",
                    Role = User.FindFirst(ClaimTypes.Role)?.Value ?? ""
                };
                var response = await _userGrpcClient.GetUserByIdAsync(request);
                var user = new
                {
                    Id = response.User.Id,
                    FirstName = response.User.FirstName,
                    LastName = response.User.LastName,
                    Email = response.User.Email,
                    CreatedAt = response.User.CreatedAt?.ToDateTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                };
                return Ok(user);
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().Contains("no autenticado"))
                {
                    return Unauthorized(new { error = ex.Message });
                }
                if (ex.Message.ToLower().Contains("error en el sistema"))
                {
                    return StatusCode(500, new { error = "Error en el sistema, intente más tarde" });
                }
                if (ex.Message.ToLower().Contains("no encontrado"))
                {
                    return NotFound(new { error = ex.Message });
                }
                if (ex.Message.ToLower().Contains("no tienes permisos"))
                {
                    return StatusCode(403, new { error = "No tienes permisos para realizar esta acción" });
                }
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("usuarios")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                request.UserId = User.FindFirst("Id")?.Value ?? "";
                request.UserEmail = User.FindFirst("Email")?.Value ?? "";  
                request.UserRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";     
                var response = await _userGrpcClient.CreateUserAsync(request);
                var user = new
                {
                    Id = response.Id,
                    FirstName = response.FirstName,
                    LastName = response.LastName,
                    Email = response.Email,
                    RoleName = response.RoleName,
                    CreatedAt = response.CreatedAt?.ToDateTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    UpdatedAt = response.UpdatedAt?.ToDateTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    IsActive = response.IsActive
                };
                return CreatedAtAction(nameof(GetUserById), new { id = response.Id }, user);
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().Contains("el correo electrónico ya está registrado"))
                {
                    return Conflict(new { error = "El correo electrónico ya está registrado" });
                }
                if (ex.Message.ToLower().Contains("no autenticado"))
                {
                    return Unauthorized(new { error = ex.Message });
                }
                if(ex.Message.ToLower().Contains("error en el sistema"))
                {
                    return StatusCode(500, new { error = "Error en el sistema, intente más tarde" });
                }
                if(ex.Message.ToLower().Contains("no encontrado"))
                {
                    return NotFound(new { error = ex.Message });
                }
                if (ex.Message.ToLower().Contains("no tienes permisos"))
                {
                    return StatusCode(403, new { error = "No tienes permisos para realizar esta acción" });
                }
                else
                {
                    return BadRequest(new { error = ex.Message });
                }
            }
        }

        [HttpPatch("usuarios/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateUser(string id, [FromForm] UpdateUserRequest request)
        {
            try
            {
                request.Id = id;
                request.UserId = User.FindFirst("Id")?.Value ?? "";
                request.UserEmail = User.FindFirst("Email")?.Value ?? "";
                request.Role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
                var response = await _userGrpcClient.UpdateUserAsync(request);
                var user = new
                {
                    Id = response.Id,
                    FirstName = response.FirstName,
                    LastName = response.LastName,
                    Email = response.Email,
                    RoleName = response.RoleName,
                    CreatedAt = response.CreatedAt?.ToDateTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    UpdatedAt = response.UpdatedAt?.ToDateTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    IsActive = response.IsActive
                };
                return Ok(user);
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().Contains("el correo electrónico ya está registrado"))
                {
                    return Conflict(new { error = "El correo electrónico ya está registrado" });
                }
                if (ex.Message.ToLower().Contains("no autenticado"))
                {
                    return Unauthorized(new { error = ex.Message });
                }
                if(ex.Message.ToLower().Contains("error en el sistema"))
                {
                    return StatusCode(500, new { error = "Error en el sistema, intente más tarde" });
                }
                if(ex.Message.ToLower().Contains("no encontrado"))
                {
                    return NotFound(new { error = ex.Message });
                }
                if (ex.Message.ToLower().Contains("no tienes permisos"))
                {
                    return StatusCode(403, new { error = "No tienes permisos para realizar esta acción" });
                }
                else
                {
                    return BadRequest(new { error = ex.Message });
                }
            }
        }

        [HttpDelete("usuarios/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                var userIdClaim = User.FindFirst("Id")?.Value ?? ""; 
                var request = new DeleteUserRequest
                {
                    Id = id,
                    UserId = userIdClaim,
                    UserEmail = User.FindFirst("Email")?.Value ?? "",
                    Role = User.FindFirst(ClaimTypes.Role)?.Value ?? ""
                };
                await _userGrpcClient.DeleteUserAsync(request);
                return NoContent();
            }
            catch (Exception ex)
            {
                if(ex.Message.ToLower().Contains("no autenticado"))
                {
                    return Unauthorized(new { error = ex.Message });
                }
                if(ex.Message.ToLower().Contains("error en el sistema"))
                {
                    return StatusCode(500, new { error = "Error en el sistema, intente más tarde" });
                }
                if(ex.Message.ToLower().Contains("no encontrado"))
                {
                    return NotFound(new { error = ex.Message });
                }
                if(ex.Message.ToLower().Contains("no tienes permisos"))
                {
                    return StatusCode(403, new { error = "No tienes permisos para realizar esta acción" });
                }
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}