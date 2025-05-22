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
            var verifyIfExists = await _context.Playlists.Where(p => p.PlaylistName == name && p.UserId == userId).FirstOrDefaultAsync();
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
        public async Task<Playlist> AddVideoToPlaylist(int playlistId, int videoId, int userId)
        {
            var playlist = await _context.Playlists.Where(p => p.Id == playlistId && p.UserId == userId).Include(p => p.Videos).FirstOrDefaultAsync();
            if (playlist == null) throw new Exception($"No tienes creada una playlist con este ID: {playlistId}");
            var video = await _context.Videos.Where(v => v.Id == videoId).FirstOrDefaultAsync();
            if (video == null) throw new Exception($"No existe un video con ese ID: {videoId}");
            playlist.Videos.Add(video);
            await _context.SaveChangesAsync();
            return playlist;
        }

        /// <summary>
        /// Obtiene una lista de reproducción por su ID.
        /// </summary>
        /// <param name="userId">El ID del usuario</param>
        /// <returns>La lista de reproducción correspondiente al ID proporcionado.</returns>
        public async Task<List<PlaylistDTO>> GetPlaylistsByUserId(int userId)
        {
            return await _context.Playlists
            .Where(p => p.UserId == userId)
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
            if (playlist == null) throw new Exception($"No tienes creada una playlist con este ID: {playlistId}");
            if (playlist.Videos.Count == 0) throw new Exception($"No tienes videos en esta playlist");
            return playlist.Videos.Select(v => new VideosByPlaylistDTO
            {
                VideoId = v.Id,
                VideoName = v.VideoName
            }).ToList();
        }
    }
}