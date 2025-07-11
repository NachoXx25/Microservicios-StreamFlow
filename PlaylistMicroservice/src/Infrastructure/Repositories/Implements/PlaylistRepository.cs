using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PlaylistMicroservice.src.Application.DTOs;
using PlaylistMicroservice.src.Domain.Models;
using PlaylistMicroservice.src.Infrastructure.Data;
using PlaylistMicroservice.src.Infrastructure.Repositories.Interfaces;

namespace PlaylistMicroservice.src.Infrastructure.Repositories.Implements
{
    public class PlaylistRepository : IPlaylistRepository
    {
        private readonly DataContext _context;

        public PlaylistRepository(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Crea una nueva lista de reproducción.
        /// </summary>
        /// <param name="name">El nombre de la lista de reproducción.</param>
        /// <param name="userId">El ID del usuario que crea la lista de reproducción.</param>
        /// <returns>La nueva lista de reproducción creada.</returns>
        public async Task<Playlist> CreatePlaylist(string name, int userId)
        {
            var verifyIfExists = await _context.Playlists.Where(p => p.PlaylistName == name && p.UserId == userId && !p.IsDeleted).FirstOrDefaultAsync();
            if (verifyIfExists != null) throw new Exception($"Ya existe una lista de reproducción con ese nombre: {name}");
            Playlist playlist = new Playlist
            {
                PlaylistName = name,
                UserId = userId,
                Videos = new List<Video>()
            };
            await _context.Playlists.AddAsync(playlist);
            await _context.SaveChangesAsync();
            return playlist;
        }


        /// <summary>
        /// Obtiene una lista de reproducción por su ID.
        /// </summary>
        /// <param name="playlistId">El ID de la lista de reproducción.</param>
        /// <param name="videoId">El ID del video a agregar.</param>
        /// <param name="userId">El ID del usuario que crea la lista de reproducción.</param>
        /// <returns>La lista de reproducción actualizada.</returns>
        public async Task<PlaylistWithVideosDTO> AddVideoToPlaylist(int playlistId, string videoId, int userId)
        {
            var playlist = await _context.Playlists.Where(p => p.Id == playlistId && p.UserId == userId).Include(p => p.Videos).FirstOrDefaultAsync();
            if (playlist == null) throw new Exception($"No encontrado: No tienes creada una playlist con este ID: {playlistId}"); 
            if (playlist.IsDeleted) throw new Exception($"La playlist con ID: {playlistId} está eliminada");
            var video = await _context.Videos.Where(v => v.Id == videoId).FirstOrDefaultAsync();
            if (video == null) throw new Exception($"No encontrado: No existe un video con ese ID: {videoId}");
            if (video.IsDeleted) throw new Exception($"El video con ID: {videoId} está eliminado");
            if (playlist.Videos.Any(v => v.Id == videoId)) throw new Exception($"Ya existe un video con ese ID en la playlist: {videoId}");
            playlist.Videos.Add(video);
            await _context.SaveChangesAsync();
            return new PlaylistWithVideosDTO
            {
                Id = playlist.Id,
                Name = playlist.PlaylistName,
                Videos = playlist.Videos.Select(v => new VideosByPlaylistDTO
                {
                    VideoId = v.Id,
                    VideoName = v.VideoName
                }).ToList()
            };
        }

        /// <summary>
        /// Obtiene una lista de reproducción por su ID.
        /// </summary>
        /// <param name="userId">El ID del usuario</param>
        /// <returns>La lista de reproducción correspondiente al ID proporcionado.</returns>
        public async Task<List<PlaylistDTO>> GetPlaylistsByUserId(int userId)
        {
            return await _context.Playlists
            .Where(p => p.UserId == userId && !p.IsDeleted)
            .Select(p => new PlaylistDTO
            {
                Id = p.Id,
                Name = p.PlaylistName
            })
            .ToListAsync();
        }

        /// <summary>
        /// Obtiene los videos de una lista de reproducción por su ID.
        /// </summary>
        /// <param name="playlistId">El ID de la lista de reproducción.</param>
        /// <param name="userId">El ID del usuario.</param>
        /// <returns>Los videos de la lista de reproducción.</returns>
        public async Task<List<VideosByPlaylistDTO>> GetVideosByPlaylistId(int playlistId, int userId)
        {
            var playlist = await _context.Playlists
                .Where(p => p.Id == playlistId && p.UserId == userId)
                .Include(p => p.Videos)
                .FirstOrDefaultAsync();
            if (playlist == null) throw new Exception($"No encontrado: No tienes creada una playlist con este ID: {playlistId}");
            if (playlist.IsDeleted) throw new Exception($"La playlist con ID: {playlistId} está eliminada");
            return playlist.Videos.Where(v => !v.IsDeleted).Select(v => new VideosByPlaylistDTO
            {
                VideoId = v.Id,
                VideoName = v.VideoName
            }).ToList();
        }

        /// <summary>
        /// Elimina una lista de reproducción por su ID.
        /// </summary>
        /// <param name="playlistId">El ID de la lista de reproducción.</param>
        /// <param name="videoId">El ID del video a eliminar.</param>
        /// <param name="userId">El ID del usuario.</param>
        /// <returns>Lista de videos actualizada.</returns>
        public async Task<PlaylistWithVideosDTO> RemoveVideoFromPlaylist(int playlistId, string videoId, int userId)
        {
            var playlist = await _context.Playlists
                .Where(p => p.Id == playlistId && p.UserId == userId)
                .Include(p => p.Videos)
                .FirstOrDefaultAsync();
            if (playlist == null) throw new Exception($"No encontrado: No tienes creada una playlist con este ID: {playlistId}");
            var video = playlist.Videos.FirstOrDefault(v => v.Id == videoId);
            if (video == null) throw new Exception($"No encontrado: No existe un video en la playlist con ese ID: {videoId}");
            if (!playlist.Videos.Any(v => v.Id == videoId)) throw new Exception($"No encontrado: La playlist no contiene un video con ID: {videoId}");
            playlist.Videos.Remove(video);
            await _context.SaveChangesAsync();
            return new PlaylistWithVideosDTO
            { 
                Id = playlist.Id,
                Name = playlist.PlaylistName,
                Videos = playlist.Videos.Select(v => new VideosByPlaylistDTO
                {
                    VideoId = v.Id,
                    VideoName = v.VideoName
                }).ToList()
            };
        }
            
        /// <summary>
        /// Elimina una lista de reproducción por su ID.
        /// </summary>
        /// <param name="playlistId">El ID de la lista de reproducción.</param>
        /// <param name="userId">El ID del usuario.</param>
        /// <returns>True si la lista de reproducción fue eliminada, de lo contrario false.</returns>
        public async Task<bool> DeletePlaylist(int playlistId, int userId)
        {
            var playlist = await _context.Playlists
                .Where(p => p.Id == playlistId && p.UserId == userId)
                .FirstOrDefaultAsync();
            if (playlist == null) return false;
            if (playlist.IsDeleted) throw new Exception($"La playlist con ID: {playlistId} ya está eliminada");
            playlist.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}