using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
    }
}