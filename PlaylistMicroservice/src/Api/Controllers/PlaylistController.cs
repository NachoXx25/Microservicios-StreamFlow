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

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetPlaylistsByUserId()
        {
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value ?? throw new Exception("Error en la autenticación del usuario");
                int id = int.Parse(userId);
                var playlists = await _playlistService.GetPlaylistsByUserId(id);
                if (playlists == null || playlists.Count == 0)
                    return NotFound(new { message = "No se encontraron listas de reproducción para este usuario." });
                return Ok(playlists);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{playlistId}")]
        [Authorize]
        public async Task<IActionResult> GetVideosByPlaylistId(int playlistId)
        {
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value ?? throw new Exception("Error en la autenticación del usuario");
                int id = int.Parse(userId);
                var videos = await _playlistService.GetVideosByPlaylistId(playlistId, id);
                if (videos == null || videos.Count == 0)
                    return NotFound(new { message = "No se encontraron videos para esta lista de reproducción." });
                return Ok(videos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("video")]
        [Authorize]
        public async Task<IActionResult> RemoveVideoFromPlaylist(RemoveVideoDTO removeVideoDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value ?? throw new Exception("Error en la autenticación del usuario");
                int id = int.Parse(userId);
                var videos = await _playlistService.RemoveVideoFromPlaylist(removeVideoDTO.PlaylistId, removeVideoDTO.VideoId, id);
                if (videos == null || videos.Count == 0)
                    return NotFound(new { message = "No se encontraron videos para esta lista de reproducción." });
                return Ok(videos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete()]
        [Authorize]
        public async Task<IActionResult> DeletePlaylist(DeletePlaylistDTO deleteDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value ?? throw new Exception("Error en la autenticación del usuario");
                int id = int.Parse(userId);
                var message = await _playlistService.DeletePlaylist(deleteDTO.PlaylistId, id);
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}