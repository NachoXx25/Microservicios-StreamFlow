using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlaylistMicroservice.src.Application.DTOs;
using PlaylistMicroservice.src.Application.Services.Interfaces;

namespace PlaylistMicroservice.src.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PlaylistController : ControllerBase
    {
        private readonly IPlaylistService _playlistService;

        public PlaylistController(IPlaylistService playlistService)
        {
            _playlistService = playlistService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreatePlaylist(CreatePlaylistDTO createPlaylist)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value ?? throw new Exception("Error en la autenticación del usuario");
                int id = int.Parse(userId);
                var playlist = await _playlistService.CreatePlaylist(createPlaylist.Name, id);
                return CreatedAtAction(nameof(CreatePlaylist), new { id = playlist.Id }, playlist);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("video")]
        [Authorize]
        public async Task<IActionResult> AddVideoToPlaylist(AddVideoToPlaylistDTO videoDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value ?? throw new Exception("Error en la autenticación del usuario");
                int id = int.Parse(userId);
                var playlist = await _playlistService.AddVideoToPlaylist(videoDTO.PlaylistId, videoDTO.VideoId, id);
                return CreatedAtAction(nameof(AddVideoToPlaylist), new { id = playlist.Id }, playlist);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}