using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiGateway.src.Application.DTOs.Video;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace ApiGateway.src.Api.Controllers
{
    [ApiController]
    [Route("")]
    public class VideoController : ControllerBase
    {
        private readonly Services.VideoGrpcClient _videoGrpcClient;

        public VideoController(Services.VideoGrpcClient videoGrpcClient)
        {
            _videoGrpcClient = videoGrpcClient;
        }

        [HttpGet("videos")]
        public async Task<IActionResult> GetAllVideos([FromQuery] GetAllVideosDTO? search)
        {
            try
            {
                var userId = User.FindFirst("Id")?.Value;
                var userEmail = User.FindFirst("Email")?.Value;
                var userRole = User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

                var request = new Protos.VideoService.GetAllVideosRequest
                {
                    UserData = new Protos.VideoService.UserData
                    {
                        Id = userId ?? "",
                        Email = userEmail ?? "",
                    }
                };

                if (!string.IsNullOrWhiteSpace(search?.Title))
                {
                    request.Title = search.Title;
                }
                if (!string.IsNullOrWhiteSpace(search?.Genre))
                {
                    request.Genre = search.Genre;
                }

                var response = await _videoGrpcClient.GetAllVideosAsync(request);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo todos los videos: {ex.Message}");
            }
        }

        [HttpGet("videos/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVideoById(string id)
        {
            try
            {
                if (!User.Identity?.IsAuthenticated == true)
                {
                    return Unauthorized(new { Error = "Se requiere autenticación para acceder a esta información." });
                }

                var userId = User.FindFirst("Id")?.Value;
                var userEmail = User.FindFirst("Email")?.Value;

                var request = new Protos.VideoService.GetVideoByIdRequest
                {
                    Id = id,
                    UserData = new Protos.VideoService.UserData
                    {
                        Id = userId ?? "",
                        Email = userEmail ?? "",
                    }
                };

                var response = await _videoGrpcClient.GetVideoByIdAsync(request);

                if (response == null)
                {
                    return NotFound(new { Message = "Video no encontrado." });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo el video con ID {id}: {ex.Message}");
            }
        }

        [HttpPost("videos")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateVideo([FromBody] UploadVideoDTO videoDto)
        {
            try
            {
                if (videoDto == null)
                {
                    return BadRequest(new { Error = "Los datos del video son inválidos." });
                }

                if (!User.Identity?.IsAuthenticated == true)
                {
                    return Unauthorized(new { Error = "Se requiere autenticación para crear un video." });
                }

                if (!User.IsInRole("Administrador"))
                {
                    return StatusCode(403, "No tienes permisos para crear un video.");
                }

                var request = new Protos.VideoService.UploadVideoRequest
                {
                    Title = videoDto.Title,
                    Description = videoDto.Description,
                    Genre = videoDto.Genre,
                    UserData = new Protos.VideoService.UserData
                    {
                        Id = User.FindFirst("Id")?.Value ?? "",
                        Email = User.FindFirst("Email")?.Value ?? "",
                    }
                };

                var response = await _videoGrpcClient.CreateVideoAsync(request);

                if (response == null)
                {
                    return StatusCode(500, "Error al crear el video.");
                }

                return StatusCode(201, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creando el video: {ex.Message}");
            }
        }

        [HttpDelete("videos/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteVideo(string id)
        {
            try
            {
                if (!User.Identity?.IsAuthenticated == true)
                {
                    return Unauthorized(new { Error = "Se requiere autenticación para eliminar un video." });
                }

                if (!User.IsInRole("Administrador"))
                {
                    return StatusCode(403, "No tienes permisos para eliminar un video.");
                }

                var userId = User.FindFirst("Id")?.Value;
                var userEmail = User.FindFirst("Email")?.Value;

                var request = new Protos.VideoService.DeleteVideoRequest
                {
                    Id = id,
                    UserData = new Protos.VideoService.UserData
                    {
                        Id = userId ?? "",
                        Email = userEmail ?? "",
                    }
                };

                var response = await _videoGrpcClient.DeleteVideoAsync(request);

                if (response == null)
                {
                    return NotFound(new { Message = "Video no encontrado o no se pudo eliminar." });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error eliminando el video con ID {id}: {ex.Message}");
            }
        }

        [HttpPatch("videos/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateVideo(string id, [FromBody] UpdateVideoDTO videoDto)
        {
            try
            {
                if (videoDto == null || (string.IsNullOrWhiteSpace(videoDto.Title) & string.IsNullOrWhiteSpace(videoDto.Description) & string.IsNullOrWhiteSpace(videoDto.Genre)))
                {
                    return BadRequest(new { Error = "Los datos del video son inválidos." });
                }

                if (!User.Identity?.IsAuthenticated == true)
                {
                    return Unauthorized(new { Error = "Se requiere autenticación para actualizar un video." });
                }

                if (!User.IsInRole("Administrador"))
                {
                    return StatusCode(403, "No tienes permisos para actualizar un video.");
                }

                var userId = User.FindFirst("Id")?.Value;
                var userEmail = User.FindFirst("Email")?.Value;

                var request = new Protos.VideoService.UpdateVideoRequest
                {
                    Id = id,
                    Title = videoDto.Title ?? "",
                    Description = videoDto.Description ?? "",
                    Genre = videoDto.Genre ?? "",
                    UserData = new Protos.VideoService.UserData
                    {
                        Id = userId ?? "",
                        Email = userEmail ?? "",
                    }
                };

                var response = await _videoGrpcClient.UpdateVideoAsync(request);

                if (response == null)
                {
                    return NotFound(new { Message = "Video no encontrado" });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error actualizando el video con ID {id}: {ex.Message}");
            }
        }
    }
}