using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ApiGateway.src.Application.DTOs
{
    public class UpdatePasswordDTO
    {
        [JsonIgnore]
        public string UserId { get; set; } = string.Empty;
        [JsonIgnore]
        public string UserEmail { get; set; } = string.Empty;
        [JsonIgnore]
        public string Jti { get; set; } = string.Empty;
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmPassword { get; set; }
    }
}