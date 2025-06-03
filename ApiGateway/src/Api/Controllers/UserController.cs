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
        public async Task<IActionResult> GetAllUsers()
        {
            var response = await _userGrpcClient.GetAllUsersAsync();
            return Ok(response.Users);
        }

        [HttpGet("usuarios/{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var response = await _userGrpcClient.GetUserByIdAsync(id);
            return Ok(response.User);
        }

        [HttpPost("usuarios")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            var response = await _userGrpcClient.CreateUserAsync(request);
            return CreatedAtAction(nameof(GetUserById), new { id = response.Id }, response);
        }

        [HttpPut("usuarios/{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserRequest request)
        {
            request.Id = id;
            var response = await _userGrpcClient.UpdateUserAsync(request);
            return Ok(response);
        }

        [HttpDelete("usuarios/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            await _userGrpcClient.DeleteUserAsync(new DeleteUserRequest { Id = id });
            return NoContent();
        }
    }
}