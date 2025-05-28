using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PlaylistMicroservice.src.Domain.Models
{
    public class Video
    {
        public required string Id { get; set; }

        public string VideoName { get; set; } = string.Empty;

        public bool IsDeleted { get; set; }
        public int? PlaylistId { get; set; }

        public Playlist? Playlist { get; set; } 
    }
}