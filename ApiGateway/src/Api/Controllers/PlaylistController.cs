using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiGateway.Protos.PlaylistService;
using ApiGateway.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
                if(User?.Identity?.IsAuthenticated != true) return Unauthorized(new { Error = "Se requiere autenticación." });
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
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("listas-reproduccion")]
        [AllowAnonymous]
        public async Task<IActionResult> CreatePlaylist([FromBody] string name)
        {
            try
            {
                if (User?.Identity?.IsAuthenticated != true) return Unauthorized(new { Error = "Se requiere autenticación." });
                if (string.IsNullOrEmpty(name))
                {
                    return BadRequest(new { Error = "El nombre de la lista de reproducción es requerido" });
                }
                var request = new CreatePlaylistRequest();
                request.Name = name;
                request.UserId = User?.FindFirst("Id")?.Value ?? request.UserId;
                request.UserEmail = User?.FindFirst("Email")?.Value ?? request.UserEmail;
                var response = await _playlistGrpcClient.CreatePlaylistAsync(request);
                return CreatedAtAction(nameof(GetPlaylistsByUserId), new { userId = request.UserId }, response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("listas-reproduccion/{id}/videos")]
        [AllowAnonymous]
        public async Task<IActionResult> AddVideoToPlaylist(string id, [FromBody] string videoId)
        {
            try
            {
                if (User?.Identity?.IsAuthenticated != true) return Unauthorized(new { Error = "Se requiere autenticación." });
                var userId = User?.FindFirst("Id")?.Value;
                var userEmail = User?.FindFirst("Email")?.Value;
                if (string.IsNullOrEmpty(videoId))
                {
                    return BadRequest(new { Error = "El ID del video es requerido" });
                }
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
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("listas-reproduccion/{id}/videos")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVideosByPlaylistId(string id)
        {
            try
            {
                if (User?.Identity?.IsAuthenticated != true) return Unauthorized(new { Error = "Se requiere autenticación." });
                var request = new GetVideosByPlaylistIdRequest();
                request.PlaylistId = id;
                request.UserId = User?.FindFirst("Id")?.Value;
                request.UserEmail = User?.FindFirst("Email")?.Value;
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(new { Error = "El ID de la lista de reproducción es requerido" });
                }
                var response = await _playlistGrpcClient.GetVideosByPlaylistIdAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpDelete("listas-reproduccion/{id}/videos")]
        [AllowAnonymous]
        public async Task<IActionResult> RemoveVideoFromPlaylist(string id, [FromBody] string videoId)
        {
            try
            {
                if (User?.Identity?.IsAuthenticated != true) return Unauthorized(new { Error = "Se requiere autenticación." });
                var userId = User?.FindFirst("Id")?.Value;
                var userEmail = User?.FindFirst("Email")?.Value;
                if (string.IsNullOrEmpty(videoId))
                {
                    return BadRequest(new { Error = "El ID del video es requerido" });
                }
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
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpDelete("listas-reproduccion/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeletePlaylist(string id)
        {
            try
            {
                if (User?.Identity?.IsAuthenticated != true) return Unauthorized(new { Error = "Se requiere autenticación." });
                var userId = User?.FindFirst("Id")?.Value;
                var userEmail = User?.FindFirst("Email")?.Value;
                var request = new DeletePlaylistRequest
                {
                    PlaylistId = id,
                    UserId = userId,
                    UserEmail = userEmail
                };
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(new { Error = "El ID de la lista de reproducción es requerido" });
                }
                var response = await _playlistGrpcClient.DeletePlaylistAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}