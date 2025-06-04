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
        public async Task<IActionResult> AddVideoToPlaylist(AddVideoToPlaylistDTO videoDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value ?? throw new Exception("Error en la autenticación del usuario");
                int id = int.Parse(userId);
                int.TryParse(videoDTO.PlaylistId, out int playlistId);
                var playlist = await _playlistService.AddVideoToPlaylist(playlistId, videoDTO.VideoId, id);
                return CreatedAtAction(nameof(AddVideoToPlaylist), new { id = playlist.Id }, playlist);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
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
        public async Task<IActionResult> RemoveVideoFromPlaylist(RemoveVideoDTO removeVideoDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value ?? throw new Exception("Error en la autenticación del usuario");
                int id = int.Parse(userId);
                int.TryParse(removeVideoDTO.PlaylistId, out int playlistId);
                var videos = await _playlistService.RemoveVideoFromPlaylist(playlistId, removeVideoDTO.VideoId, id);
                return Ok(videos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete()]
        public async Task<IActionResult> DeletePlaylist(DeletePlaylistDTO deleteDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value ?? throw new Exception("Error en la autenticación del usuario");
                int id = int.Parse(userId);
                int.TryParse(deleteDTO.PlaylistId, out int playlistId);
                var message = await _playlistService.DeletePlaylist(playlistId, id);
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}