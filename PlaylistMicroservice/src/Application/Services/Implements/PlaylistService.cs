using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlaylistMicroservice.src.Application.Services.Interfaces;
using PlaylistMicroservice.src.Domain.Models;
using PlaylistMicroservice.src.Infrastructure.Repositories.Interfaces;
using Serilog;

namespace PlaylistMicroservice.src.Application.Services.Implements
{
    public class PlaylistService : IPlaylistService
    {
        private readonly IPlaylistRepository _playlistRepository;
        public PlaylistService(IPlaylistRepository playlistRepository)
        {
            _playlistRepository = playlistRepository;
        }

        /// <summary>
        /// Crea una nueva lista de reproducción.
        /// </summary>
        /// <param name="name">El nombre de la lista de reproducción.</param>
        /// <param name="userId">El ID del usuario que crea la lista de reproducción.</param>
        /// <returns>La nueva lista de reproducción creada.</returns>
        public async Task<Playlist> CreatePlaylist(string name, int userId)
        {
            try
            {
                return await _playlistRepository.CreatePlaylist(name, userId);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene una lista de reproducción por su ID.
        /// </summary>
        /// <param name="playlistId">El ID de la lista de reproducción.</param>
        /// <param name="videoId">El ID del video a agregar.</param>
        /// <param name="userId">El ID del usuario que crea la lista de reproducción.</param>
        /// <returns>La lista de reproducción correspondiente al ID proporcionado.</returns>
        public Task<Playlist> AddVideoToPlaylist(int playlistId, int videoId, int userId)
        {
            try
            {
                return _playlistRepository.AddVideoToPlaylist(playlistId, videoId, userId);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }
    }
}