using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthMicroservice.src.Application.DTOs
{
    public class TokenValidationResponseDTO
    {
        public bool IsBlacklisted { get; set; }
        public string? Message { get; set; }
    }
}