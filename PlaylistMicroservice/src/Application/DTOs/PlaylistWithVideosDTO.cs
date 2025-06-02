using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlaylistMicroservice.src.Application.DTOs
{
    public class PlaylistWithVideosDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<VideosByPlaylistDTO> Videos { get; set; } = new();
    }
}