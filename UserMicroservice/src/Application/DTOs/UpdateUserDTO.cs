using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UserMicroservice.src.Application.DTOs
{
    public class UpdateUserDTO
    {
        [MinLength(2, ErrorMessage ="El nombre debe tener mínimo 2 letras.")]
        [MaxLength(20, ErrorMessage ="El nombre debe tener máximo 20 letras.")]
        public string? FirstName { get; set; }
        [MinLength(2, ErrorMessage ="El apellido debe tener mínimo 2 letras.")]
        [MaxLength(20, ErrorMessage ="El apellido debe tener máximo 20 letras.")]
        public string? LastName { get; set; }
        [RegularExpression (@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "El Correo electrónico no es válido.")]
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}