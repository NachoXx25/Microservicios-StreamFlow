using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlaylistMicroservice.src.Application.DTOs
{
    public class VideosByPlaylistDTO
    {
        public string VideoId { get; set; } = string.Empty;
        public required string VideoName { get; set; } 

    }
}