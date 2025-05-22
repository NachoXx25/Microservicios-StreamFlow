using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlaylistMicroservice.src.Domain.Models;

namespace PlaylistMicroservice.src.Application.Services.Interfaces
{
    public interface IPlaylistService
    {
        /// <summary>
        /// Crea una nueva lista de reproducción.
        /// </summary>
        /// <param name="name">El nombre de la lista de reproducción.</param>
        /// <param name="userId">El ID del usuario que crea la lista de reproducción.</param>
        /// <returns>La nueva lista de reproducción creada.</returns>
        Task<Playlist> CreatePlaylist(string name, int userId);
    }
}