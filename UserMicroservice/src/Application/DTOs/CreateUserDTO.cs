using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UserMicroservice.src.Application.DTOs
{
    public class CreateUserDTO
    {
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s\-]+$", ErrorMessage = "El Nombre solo puede contener carácteres del abecedario español.")]
        [MinLength(2, ErrorMessage ="El nombre debe tener mínimo 2 letras.")]
        [MaxLength(20, ErrorMessage ="El nombre debe tener máximo 20 letras.")]
        public required string FirstName { get; set; }
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s\-]+$", ErrorMessage = "El Apellido solo puede contener carácteres del abecedario español.")]
        [MinLength(2, ErrorMessage ="El apellido debe tener mínimo 2 letras.")]
        [MaxLength(20, ErrorMessage ="El apellido debe tener máximo 20 letras.")]
        public required string LastName { get; set; }
        [Required(ErrorMessage = "El email es requerido")]
        [RegularExpression (@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "El Correo electrónico no es válido.")]
        public required string Email { get; set; }
        [Required(ErrorMessage ="La contraseña es requerida.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[0-9])(?=.*[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ])[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ0-9]+$", ErrorMessage = "La contraseña debe ser alfanumérica y contener al menos una mayúscula.")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")] 
        [MaxLength(20, ErrorMessage = "La contraseña debe tener como máximo 20 caracteres")]
        public required string Password { get; set; }
        [Required(ErrorMessage ="La confirmación de la contraseña es requerida.")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public required string ConfirmPassword { get; set; }
        [Required(ErrorMessage = "El rol es requerido")]
        public required string Role { get; set; }
    }
}