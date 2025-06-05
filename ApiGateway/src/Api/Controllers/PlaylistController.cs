using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiGateway.Protos.PlaylistService;
using ApiGateway.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace ApiGateway.src.Api.Controllers
{
    [ApiController]
    [Route("")]
    public class PlaylistController : ControllerBase
    {
        private readonly PlaylistGrpcClient _playlistGrpcClient;

        public PlaylistController(PlaylistGrpcClient playlistGrpcClient)
        {
            _playlistGrpcClient = playlistGrpcClient;
        }

        [HttpGet("listas-reproduccion")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPlaylistsByUserId()
        {
            try
            {
                var userId = User?.FindFirst("Id")?.Value;
                var userEmail = User?.FindFirst("Email")?.Value;
                var request = new GetPlaylistsByUserIdRequest();
                request.UserId = userId;
                request.UserEmail = userEmail;
                var response = await _playlistGrpcClient.GetPlaylistsByUserIdAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                
                if(ex.Message.ToLower().Contains("no autenticado"))
                {
                    return Unauthorized(new { error = "No autenticado" });
                }
                if(ex.Message.ToLower().Contains("error en el sistema"))
                {
                    return StatusCode(500, new { error = "Error en el sistema, intente más tarde" });
                }
                if(ex.Message.ToLower().Contains("no encontrado"))
                {
                    return NotFound(new { error = "no encontrado" });
                }
                if(ex.Message.ToLower().Contains("no tienes permisos"))
                {
                    return StatusCode(403, new { error = "No tienes permisos para realizar esta acción" });
                }
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("listas-reproduccion")]
        [AllowAnonymous]
        public async Task<IActionResult> CreatePlaylist([FromForm] string name)
        { 
            try
            {
                if (User?.Identity?.IsAuthenticated != true) return Unauthorized(new { Error = "Se requiere autenticación." });
                var request = new CreatePlaylistRequest();
                Log.Information("Creando lista de reproducción: {Name}", name);
                request.Name = name;
                request.UserId = User?.FindFirst("Id")?.Value ?? request.UserId;
                request.UserEmail = User?.FindFirst("Email")?.Value ?? request.UserEmail;
                var response = await _playlistGrpcClient.CreatePlaylistAsync(request);
                return CreatedAtAction(nameof(GetPlaylistsByUserId), response);
            }
            catch (Exception ex)
            {
                if(ex.Message.ToLower().Contains("no autenticado"))
                {
                    return Unauthorized(new { error = "No autenticado" });
                }
                if(ex.Message.ToLower().Contains("error en el sistema"))
                {
                    return StatusCode(500, new { error = "Error en el sistema, intente más tarde" });
                }
                if(ex.Message.ToLower().Contains("no encontrado"))
                {
                    return NotFound(new { error = "no encontrado" });
                }
                if(ex.Message.ToLower().Contains("no tienes permisos"))
                {
                    return StatusCode(403, new { error = "No tienes permisos para realizar esta acción" });
                }
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("listas-reproduccion/{id}/videos")]
        [AllowAnonymous]
        public async Task<IActionResult> AddVideoToPlaylist(string id, [FromForm] string videoId)
        {
            try
            {
                var userId = User?.FindFirst("Id")?.Value;
                var userEmail = User?.FindFirst("Email")?.Value;
                var request = new AddVideoToPlaylistRequest();
                request.VideoId = videoId;
                request.UserId = userId;
                request.UserEmail = userEmail;
                request.PlaylistId = id;
                var response = await _playlistGrpcClient.AddVideoToPlaylistAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                if(ex.Message.ToLower().Contains("no autenticado"))
                {
                    return Unauthorized(new { error = "No autenticado" });
                }
                if(ex.Message.ToLower().Contains("error en el sistema"))
                {
                    return StatusCode(500, new { error = "Error en el sistema, intente más tarde" });
                }
                if(ex.Message.ToLower().Contains("no encontrado"))
                {
                    return NotFound(new { error = "no encontrado" });
                }
                if(ex.Message.ToLower().Contains("no tienes permisos"))
                {
                    return StatusCode(403, new { error = "No tienes permisos para realizar esta acción" });
                }
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("listas-reproduccion/{id}/videos")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVideosByPlaylistId(string id)
        {
            try
            {
                var request = new GetVideosByPlaylistIdRequest();
                request.PlaylistId = id;
                request.UserId = User?.FindFirst("Id")?.Value;
                request.UserEmail = User?.FindFirst("Email")?.Value;
                var response = await _playlistGrpcClient.GetVideosByPlaylistIdAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                if(ex.Message.ToLower().Contains("no autenticado"))
                {
                    return Unauthorized(new { error = "No autenticado" });
                }
                if(ex.Message.ToLower().Contains("error en el sistema"))
                {
                    return StatusCode(500, new { error = "Error en el sistema, intente más tarde" });
                }
                if(ex.Message.ToLower().Contains("no encontrado"))
                {
                    return NotFound(new { error = "no encontrado" });
                }
                if(ex.Message.ToLower().Contains("no tienes permisos"))
                {
                    return StatusCode(403, new { error = "No tienes permisos para realizar esta acción" });
                }
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("listas-reproduccion/{id}/videos")]
        [AllowAnonymous]
        public async Task<IActionResult> RemoveVideoFromPlaylist(string id, [FromForm] string videoId)
        {
            try
            {
                var userId = User?.FindFirst("Id")?.Value;
                var userEmail = User?.FindFirst("Email")?.Value;
                var request = new RemoveVideoFromPlaylistRequest
                {
                    UserId = userId,
                    UserEmail = userEmail,
                    PlaylistId = id,
                    VideoId = videoId
                };
                var response = await _playlistGrpcClient.RemoveVideoFromPlaylistAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                if(ex.Message.ToLower().Contains("no autenticado"))
                {
                    return Unauthorized(new { error = "No autenticado" });
                }
                if(ex.Message.ToLower().Contains("error en el sistema"))
                {
                    return StatusCode(500, new { error = "Error en el sistema, intente más tarde" });
                }
                if(ex.Message.ToLower().Contains("no encontrado"))
                {
                    return NotFound(new { error = "no encontrado" });
                }
                if(ex.Message.ToLower().Contains("no tienes permisos"))
                {
                    return StatusCode(403, new { error = "No tienes permisos para realizar esta acción" });
                }
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("listas-reproduccion/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeletePlaylist(string id)
        {
            try
            {
                var userId = User?.FindFirst("Id")?.Value;
                var userEmail = User?.FindFirst("Email")?.Value;
                var request = new DeletePlaylistRequest
                {
                    PlaylistId = id,
                    UserId = userId,
                    UserEmail = userEmail
                };
                var response = await _playlistGrpcClient.DeletePlaylistAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                if(ex.Message.ToLower().Contains("no autenticado"))
                {
                    return Unauthorized(new { error = "No autenticado" });
                }
                if(ex.Message.ToLower().Contains("error en el sistema"))
                {
                    return StatusCode(500, new { error = "Error en el sistema, intente más tarde" });
                }
                if(ex.Message.ToLower().Contains("no encontrado"))
                {
                    return NotFound(new { error = "no encontrado" });
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