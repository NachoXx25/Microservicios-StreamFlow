using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlaylistMicroservice.src.Application.DTOs;
using PlaylistMicroservice.src.Domain.Models;

namespace PlaylistMicroservice.src.Infrastructure.Repositories.Interfaces
{
    public interface IPlaylistRepository
    {
        /// <summary>
        /// Crea una nueva lista de reproducción.
        /// </summary>
        /// <param name="name">El nombre de la lista de reproducción.</param>
        /// <param name="userId">El ID del usuario que crea la lista de reproducción.</param>
        /// <returns>La nueva lista de reproducción creada.</returns>
        Task<Playlist> CreatePlaylist(string name, int userId);

        /// <summary>
        /// Obtiene una lista de reproducción por su ID.
        /// </summary>
        /// <param name="playlistId">El ID de la lista de reproducción.</param>
        /// <param name="videoId">El ID del video a agregar.</param>
        /// <param name="userId">El ID del usuario que crea la lista de reproducción.</param>
        /// <returns>La lista de reproducción actualizada.</returns>
        Task<PlaylistWithVideosDTO> AddVideoToPlaylist(int playlistId, string videoId, int userId);

        /// <summary>
        /// Obtiene una lista de reproducción por su ID.
        /// </summary>
        /// <param name="userId">El ID del usuario</param>
        /// <returns>La lista de reproducción correspondiente al ID proporcionado.</returns>
        Task<List<PlaylistDTO>> GetPlaylistsByUserId(int userId);

        /// <summary>
        /// Obtiene los videos de una lista de reproducción por su ID.
        /// </summary>
        /// <param name="playlistId">El ID de la lista de reproducción.</param>
        /// <param name="userId">El ID del usuario.</param>
        /// <returns>Los videos de la lista de reproducción.</returns>
        Task<List<VideosByPlaylistDTO>> GetVideosByPlaylistId(int playlistId, int userId);

        /// <summary>
        /// Elimina una lista de reproducción por su ID.
        /// </summary>
        /// <param name="playlistId">El ID de la lista de reproducción.</param>
        /// <param name="videoId">El ID del video a eliminar.</param>
        /// <param name="userId">El ID del usuario.</param>
        /// <returns>La lista de reproducción actualizada.</returns>
        Task<PlaylistWithVideosDTO> RemoveVideoFromPlaylist(int playlistId, string videoId, int userId);

        /// <summary>
        /// Elimina una lista de reproducción por su ID.
        /// </summary>
        /// <param name="playlistId">El ID de la lista de reproducción.</param>
        /// <param name="userId">El ID del usuario.</param>
        /// <returns>True si la lista de reproducción fue eliminada, de lo contrario false.</returns>
        Task<bool> DeletePlaylist(int playlistId, int userId);
    }
}