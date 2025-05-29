using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VideoMicroservice.src.Application.DTOs
{
    public class UpdateVideoDTO
    {
        public string? Title { get; set; }
        
        public string? Description { get; set; }

        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s\-]+$", ErrorMessage = "El Género solo puede contener carácteres del abecedario español.")]
        public string? Genre { get; set; }
    }
}