using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ApiGateway.src.Application.DTOs.Video;
using Grpc.Core;
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
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var request = new Protos.VideoService.GetAllVideosRequest
                {
                    UserData = new Protos.VideoService.UserData
                    {
                        Id = userId ?? "",
                        Email = userEmail ?? "",
                        Role = userRole ?? ""
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
            catch (RpcException ex)
            {
                var errorMessage = ex.Status.Detail.ToLower();

                if (errorMessage.Contains("no autenticado"))
                {
                    return Unauthorized(new { error = ex.Status.Detail });
                }
                if (errorMessage.Contains("error en el sistema"))
                {
                    return StatusCode(500, new { error = "Error en el sistema, intente más tarde" });
                }
                if (errorMessage.Contains("no encontrado"))
                {
                    return NotFound(new { error = ex.Status.Detail });
                }
                if (errorMessage.Contains("no tienes permisos"))
                {
                    return StatusCode(403, new { error = "No tienes permisos para realizar esta acción" });
                }
                return BadRequest(new { error = ex.Status.Detail });
            }
        }

        [HttpGet("videos/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVideoById(string id)
        {
            try
            {
                var userId = User.FindFirst("Id")?.Value;
                var userEmail = User.FindFirst("Email")?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var request = new Protos.VideoService.GetVideoByIdRequest
                {
                    Id = id,
                    UserData = new Protos.VideoService.UserData
                    {
                        Id = userId ?? "",
                        Email = userEmail ?? "",
                        Role = userRole ?? ""
                    }
                };

                var response = await _videoGrpcClient.GetVideoByIdAsync(request);

                if (response == null)
                {
                    return NotFound(new { Message = "Video no encontrado." });
                }

                return Ok(response);
            }
            catch (RpcException ex)
            {
                var errorMessage = ex.Status.Detail.ToLower();

                if (errorMessage.Contains("no autenticado"))
                {
                    return Unauthorized(new { error = ex.Status.Detail });
                }
                if (errorMessage.Contains("error en el sistema"))
                {
                    return StatusCode(500, new { error = "Error en el sistema, intente más tarde" });
                }
                if (errorMessage.Contains("no encontrado"))
                {
                    return NotFound(new { error = ex.Status.Detail });
                }
                if (errorMessage.Contains("no tienes permisos"))
                {
                    return StatusCode(403, new { error = "No tienes permisos para realizar esta acción" });
                }
                return BadRequest(new { error = ex.Status.Detail });
            }
        }

        [HttpPost("videos")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateVideo([FromBody] UploadVideoDTO videoDto)
        {
            try
            {
                var userId = User.FindFirst("Id")?.Value;
                var userEmail = User.FindFirst("Email")?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var request = new Protos.VideoService.UploadVideoRequest
                {
                    Title = videoDto.Title,
                    Description = videoDto.Description,
                    Genre = videoDto.Genre,
                    UserData = new Protos.VideoService.UserData
                    {
                        Id = userId ?? "",
                        Email = userEmail ?? "",
                        Role = userRole ?? ""
                    }
                };

                var response = await _videoGrpcClient.CreateVideoAsync(request);

                return StatusCode(201, response);
            }
            catch (RpcException ex)
            {
                var errorMessage = ex.Status.Detail.ToLower();

                if (errorMessage.Contains("no autenticado"))
                {
                    return Unauthorized(new { error = ex.Status.Detail });
                }
                if (errorMessage.Contains("error en el sistema"))
                {
                    return StatusCode(500, new { error = "Error en el sistema, intente más tarde" });
                }
                if (errorMessage.Contains("no encontrado"))
                {
                    return NotFound(new { error = ex.Status.Detail });
                }
                if (errorMessage.Contains("no tienes permisos"))
                {
                    return StatusCode(403, new { error = "No tienes permisos para realizar esta acción" });
                }
                return BadRequest(new { error = ex.Status.Detail });
            }
        }

        [HttpDelete("videos/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteVideo(string id)
        {
            try
            {
                var userId = User.FindFirst("Id")?.Value;
                var userEmail = User.FindFirst("Email")?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var request = new Protos.VideoService.DeleteVideoRequest
                {
                    Id = id,
                    UserData = new Protos.VideoService.UserData
                    {
                        Id = userId ?? "",
                        Email = userEmail ?? "",
                        Role = userRole ?? ""
                    }
                };

                var response = await _videoGrpcClient.DeleteVideoAsync(request);

                return NoContent();
            }
            catch (RpcException ex)
            {
                var errorMessage = ex.Status.Detail.ToLower();

                if (errorMessage.Contains("no autenticado"))
                {
                    return Unauthorized(new { error = ex.Status.Detail });
                }
                if (errorMessage.Contains("error en el sistema"))
                {
                    return StatusCode(500, new { error = "Error en el sistema, intente más tarde" });
                }
                if (errorMessage.Contains("no encontrado"))
                {
                    return NotFound(new { error = ex.Status.Detail });
                }
                if (errorMessage.Contains("no tienes permisos"))
                {
                    return StatusCode(403, new { error = "No tienes permisos para realizar esta acción" });
                }
                return BadRequest(new { error = ex.Status.Detail });
            }
        }

        [HttpPatch("videos/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateVideo(string id, [FromBody] UpdateVideoDTO videoDto)
        {
            try
            {
                var userId = User.FindFirst("Id")?.Value;
                var userEmail = User.FindFirst("Email")?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

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
                        Role = userRole ?? ""
                    }
                };

                var response = await _videoGrpcClient.UpdateVideoAsync(request);

                return Ok(response);
            }
            catch (RpcException ex)
            {
                var errorMessage = ex.Status.Detail.ToLower();

                if (errorMessage.Contains("no autenticado"))
                {
                    return Unauthorized(new { error = ex.Status.Detail });
                }
                if (errorMessage.Contains("error en el sistema"))
                {
                    return StatusCode(500, new { error = "Error en el sistema, intente más tarde" });
                }
                if (errorMessage.Contains("no encontrado"))
                {
                    return NotFound(new { error = ex.Status.Detail });
                }
                if (errorMessage.Contains("no tienes permisos"))
                {
                    return StatusCode(403, new { error = "No tienes permisos para realizar esta acción" });
                }
                return BadRequest(new { error = ex.Status.Detail });
            }
        }
    }
}