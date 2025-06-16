using System.ComponentModel.DataAnnotations;

namespace AuthMicroservice.src.Application.DTOs
{
    public class LoginDTO
    {

        public required string Email { get; set; }

        public required string Password { get; set; }
    }
}