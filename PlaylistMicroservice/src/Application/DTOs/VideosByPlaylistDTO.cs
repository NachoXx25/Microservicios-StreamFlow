using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlaylistMicroservice.src.Application.DTOs
{
    public class VideosByPlaylistDTO
    {
        public int VideoId { get; set; }
        public required string VideoName { get; set; } 

    }
}