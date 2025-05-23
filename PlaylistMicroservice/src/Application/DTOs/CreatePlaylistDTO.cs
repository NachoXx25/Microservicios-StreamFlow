using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PlaylistMicroservice.src.Application.DTOs
{
    public class CreatePlaylistDTO
    {   
        [Required(ErrorMessage = "El nombre de la lista de reproducción es requerido")]
        public required string Name { get; set; } 
    }
}