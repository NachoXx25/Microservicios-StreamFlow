using System.ComponentModel.DataAnnotations;

namespace AuthMicroservice.src.Application.DTOs
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "El email es requerido")]
        [RegularExpression (@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "El Correo electrónico no es válido.")]
        public required string Email { get; set; }

        [Required(ErrorMessage ="La contraseña es requerida.")]
        public required string Password { get; set; }
    }
}