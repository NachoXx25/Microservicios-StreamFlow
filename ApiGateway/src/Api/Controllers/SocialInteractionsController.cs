using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiGateway.Services;
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

                if (User.Identity?.IsAuthenticated == false)
                {
                    return Unauthorized(new { Message = "Usuario no autenticado" });
                }

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
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error cargando las interacciones del video", Error = ex.Message });
            }
        }

        [HttpPost("interacciones/{id}/likes")]
        [AllowAnonymous]
        public async Task<IActionResult> GiveLike(string id)
        {
            try
            {

                if (User.Identity?.IsAuthenticated == false)
                {
                    return Unauthorized(new { Message = "Usuario no autenticado" });
                }

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

                if (response == null)
                {
                    return BadRequest(new { Message = "Error al dar like al video." });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error dando like al video", Error = ex.Message });
            }
        }

        [HttpPost("interacciones/{id}/comentarios")]
        [AllowAnonymous]
        public async Task<IActionResult> MakeComment(string id, [FromBody]string comment)
        {
            try
            {
                if (User.Identity?.IsAuthenticated == false)
                {
                    return Unauthorized(new { Message = "Usuario no autenticado" });
                }

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

                if (response == null)
                {
                    return BadRequest(new { Message = "Error al agregar el comentario al video." });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error agregando comentario al video", Error = ex.Message });
            }
        }
    }
}