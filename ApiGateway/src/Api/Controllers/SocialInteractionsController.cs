using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiGateway.Services;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.src.Api.Controllers
{
    [ApiController]
    [Route("")]
    public class SocialInteractionsController : ControllerBase
    {
        private readonly SocialInteractionsGrpcClient _socialInteractionsGrpcClient;

        public SocialInteractionsController(SocialInteractionsGrpcClient socialInteractionsGrpcClient)
        {
            _socialInteractionsGrpcClient = socialInteractionsGrpcClient;
        }

        [HttpGet("interacciones/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVideoLikesAndComments(string id)
        {
            try
            {
                var userId = User.FindFirst("Id")?.Value;
                var userEmail = User.FindFirst("Email")?.Value;

                var request = new Protos.SocialInteractionsService.GetVideoLikesAndCommentsRequest
                {
                    VideoId = id,
                    UserData = new Protos.SocialInteractionsService.UserData
                    {
                        Id = userId ?? "",
                        Email = userEmail ?? ""
                    }
                };

                var response = await _socialInteractionsGrpcClient.GetVideoLikesAndCommentsAsync(request);

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

        [HttpPost("interacciones/{id}/likes")]
        [AllowAnonymous]
        public async Task<IActionResult> GiveLike(string id)
        {
            try
            {
                var userId = User.FindFirst("Id")?.Value;
                var userEmail = User.FindFirst("Email")?.Value;

                var request = new Protos.SocialInteractionsService.GiveLikeRequest
                {
                    VideoId = id,
                    UserData = new Protos.SocialInteractionsService.UserData
                    {
                        Id = userId ?? "",
                        Email = userEmail ?? ""
                    }
                };

                var response = await _socialInteractionsGrpcClient.GiveLikeAsync(request);
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

        [HttpPost("interacciones/{id}/comentarios")]
        [AllowAnonymous]
        public async Task<IActionResult> MakeComment(string id, [FromBody]string comment)
        {
            try
            {
                var userId = User.FindFirst("Id")?.Value;
                var userEmail = User.FindFirst("Email")?.Value;

                var request = new Protos.SocialInteractionsService.MakeCommentRequest
                {
                    VideoId = id,
                    Comment = comment,
                    UserData = new Protos.SocialInteractionsService.UserData
                    {
                        Id = userId ?? "",
                        Email = userEmail ?? ""
                    }
                };

                var response = await _socialInteractionsGrpcClient.MakeCommentAsync(request);

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