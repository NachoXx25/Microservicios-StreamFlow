using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VideoMicroservice.src.Application.DTOs
{
    public class UploadVideoDTO
    {
        [Required(ErrorMessage = "El título es requerido.")]
        public required string Title { get; set; }

        [Required(ErrorMessage = "La descripción es requerida.")]        
        public required string Description { get; set; }

        [Required(ErrorMessage = "El género es requerido.")]
        public required string Genre { get; set; }
    }
}