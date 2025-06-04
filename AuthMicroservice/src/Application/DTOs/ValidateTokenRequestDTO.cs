using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthMicroservice.src.Application.DTOs
{
    public class ValidateTokenRequestDTO
    {
        public string Token { get; set; } = string.Empty;

        public string? UserId { get; set; } = null;

        public string? UserEmail { get; set; } = null;
    }
}