using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PlaylistMicroservice.src.Application.DTOs
{
    public class AddVideoToPlaylistDTO
    {
        [Required(ErrorMessage = "El ID del video es requerido")]
        public string VideoId { get; set; } = string.Empty;
        [Required(ErrorMessage = "El ID de la lista de reproducción es requerido")]
        [RegularExpression(@"^\d+$", ErrorMessage = "El ID de la lista de reproducción debe ser un número entero positivo")]
        public string PlaylistId { get; set; } = string.Empty;
    }
}