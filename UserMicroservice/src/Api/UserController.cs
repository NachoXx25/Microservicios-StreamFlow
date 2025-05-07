using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserMicroservice.src.Application.DTOs;
using UserMicroservice.src.Application.Services.Interfaces;

namespace UserMicroservice.src.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Obtiene todos los usuarios.
        /// </summary>
        /// <param name="search">Filtro de busqueda.</param>
        /// <returns>Lista de usuarios.</returns>
        [HttpGet("usuarios")]
        [Authorize (Roles = "Administrador" )]
        public async Task<IActionResult> GetAllUsers([FromQuery] SearchByDTO search)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                return Ok(await _userService.GetAllUsers(search));
            }catch(Exception ex)
            {
                return BadRequest(new { Error = ex.Message});
            }
        }

        /// <summary>
        /// Obtiene los datos de un usuario en función de su Id
        /// </summary>
        /// <param name="Id">Id del usuario.</param>
        /// <returns>Los datos del usuario</returns> 
        [HttpGet("usuarios/{Id}")]
        [Authorize( Roles = "Administrador" )]
        public async Task<IActionResult> GetUserById(int Id)
        {
            try{
                return Ok(await _userService.GetUserById(Id));
            }catch(Exception ex)
            {
                return NotFound(new { Error = ex.Message});
            }
        }
        
        /// <summary>
        /// Endpoint para crear un nuevo usuario.
        /// </summary>
        /// <param name="userDTO">Datos del usuario.</param>
        /// <returns>Ok en caso de exito o bad request en caso de errror.</returns>
        [HttpPost("usuarios")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUser(CreateUserDTO userDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }         
                if (userDTO.Role?.ToLower() == "administrador")
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
                var user = await _userService.CreateUser(userDTO);
                return Ok(user);
            }catch(Exception ex)
            {
                return BadRequest( new { Error = ex.Message});
            }
        }
        
        [HttpPatch("usuarios/{Id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser([FromForm] UpdateUserDTO updateUserDTO, int Id)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var userIdClaim = User.FindFirst("Id")?.Value;
                if(userIdClaim != Id.ToString()) throw new Exception("No puedes editar a otros usuarios.");
                if(!string.IsNullOrEmpty(updateUserDTO.Password)) throw new Exception("No puedes editar la contraseña campo aquí.");
                return Ok(new { user = await _userService.UpdateUser(updateUserDTO, Id)});
            }catch(Exception ex)
            {
                return BadRequest( new { Error = ex.Message});
            }
        }
        
        [HttpDelete("usuarios/{Id}")]
        [Authorize( Roles = "Administrador" )]
        public async Task<IActionResult> DeleteUser(int Id)
        {
            try
            {
                var userIdClaim = User.FindFirst("Id")?.Value;
                if(userIdClaim == Id.ToString()){
                    throw new Exception("No puedes eliminarte a ti mismo.");
                }
                await _userService.DeleteUser(Id);
                return NoContent();
            }catch(Exception ex)
            {
                return BadRequest( new { Error = ex.Message });
            }
        }
    }
}