using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ApiGateway.src.Application.DTOs
{
    public class LoginRequestDTO
    {
        [JsonIgnore]
        public string UserId { get; set; } = string.Empty;
        [JsonIgnore]
        public string UserEmail { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}