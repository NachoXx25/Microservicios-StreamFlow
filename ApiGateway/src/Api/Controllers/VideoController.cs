using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiGateway.Protos.VideoService;
using ApiGateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.src.Api.Controllers
{
    [ApiController]
    [Route("")]
    public class VideoController : ControllerBase
    {
        private readonly VideoGrpcClient _videoGrpcClient;

        public VideoController(VideoGrpcClient videoGrpcClient)
        {
            _videoGrpcClient = videoGrpcClient;
        }

        [HttpGet("videos")]
        public async Task<IActionResult> GetAllVideos()
        {
            var response = await _videoGrpcClient.GetAllVideosAsync();
            return Ok(response.Videos);
        }

        [HttpGet("videos/{id}")]
        public async Task<IActionResult> GetVideoById(string id)
        {
            var response = await _videoGrpcClient.GetVideoByIdAsync(id);
            return Ok(response);
        }

        [HttpPost("videos")]
        public async Task<IActionResult> UploadVideo([FromBody] UploadVideoRequest request)
        {
            var response = await _videoGrpcClient.UploadVideoAsync(request);
            return CreatedAtAction(nameof(GetVideoById), new { id = response.Id }, response);
        }

        [HttpPatch("videos/{id}")]
        public async Task<IActionResult> UpdateVideo(string id, [FromBody] UpdateVideoRequest request)
        {
            request.Id = id;
            var response = await _videoGrpcClient.UpdateVideoAsync(request);
            return Ok(response);
        }

        [HttpDelete("videos/{id}")]
        public async Task<IActionResult> DeleteVideo(string id)
        {
            await _videoGrpcClient.DeleteVideoAsync(new DeleteVideoRequest { Id = id });
            return NoContent();
        }
    }
}