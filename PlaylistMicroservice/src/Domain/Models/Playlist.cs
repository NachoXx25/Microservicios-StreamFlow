using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PlaylistMicroservice.src.Domain.Models
{
    public class Playlist
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string PlaylistName { get; set; } = string.Empty;

        public ICollection<Video> Videos { get; set; } = [];
    }
}