using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PlaylistMicroservice.src.Domain.Models
{
    public class Video
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string VideoName { get; set; } = string.Empty;

        public bool IsDeleted { get; set; }
        public int PlaylistId { get; set; }

        public Playlist Playlist { get; set; } = null!;
    }
}