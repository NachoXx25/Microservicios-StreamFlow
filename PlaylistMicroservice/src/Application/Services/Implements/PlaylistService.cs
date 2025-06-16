using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlaylistMicroservice.src.Application.DTOs;
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
        public Task<PlaylistWithVideosDTO> AddVideoToPlaylist(int playlistId, string videoId, int userId)
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

        /// <summary>
        /// Obtiene una lista de reproducción por su ID.
        /// </summary>
        /// <param name="userId">El ID del usuario</param>
        /// <returns>La lista de reproducción correspondiente al ID proporcionado.</returns>
        public async Task<List<PlaylistDTO>> GetPlaylistsByUserId(int userId)
        {
            try
            {
                return await _playlistRepository.GetPlaylistsByUserId(userId);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene los videos de una lista de reproducción por su ID.
        /// </summary>
        /// <param name="playlistId">El ID de la lista de reproducción.</param>
        /// <param name="userId">El ID del usuario.</param>
        /// <returns>Los videos de la lista de reproducción.</returns>
        public async Task<List<VideosByPlaylistDTO>> GetVideosByPlaylistId(int playlistId, int userId)
        {
            try
            {
                return await _playlistRepository.GetVideosByPlaylistId(playlistId, userId);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Elimina una lista de reproducción por su ID.
        /// </summary>
        /// <param name="playlistId">El ID de la lista de reproducción.</param>
        /// <param name="videoId">El ID del video a eliminar.</param>
        /// <param name="userId">El ID del usuario.</param>
        /// <returns>La lista de reproducción actualizada.</returns>
        public Task<PlaylistWithVideosDTO> RemoveVideoFromPlaylist(int playlistId, string videoId, int userId)
        {
            try
            {
                return _playlistRepository.RemoveVideoFromPlaylist(playlistId, videoId, userId);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Elimina una lista de reproducción por su ID.
        /// </summary>
        /// <param name="playlistId">El ID de la lista de reproducción.</param>
        /// <param name="userId">El ID del usuario.</param>
        /// <returns>Mensaje de éxito o error.</returns>
        public async Task<string> DeletePlaylist(int playlistId, int userId)
        {
            try
            {
                var result = await _playlistRepository.DeletePlaylist(playlistId, userId);
                if(result) 
                    return "Playlist eliminada correctamente";
                else
                    throw new Exception("No encontrado: No se pudo eliminar la playlist, verifique que el ID sea correcto o que no esté eliminada");
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }
    }
}