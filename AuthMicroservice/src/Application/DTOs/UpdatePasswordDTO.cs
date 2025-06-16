using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AuthMicroservice.src.Application.DTOs
{
    public class UpdatePasswordDTO
    {
        [JsonIgnore]
        public string UserId { get; set; } = string.Empty;
        [JsonIgnore]
        public string UserRequestId { get; set; } = string.Empty;
        public required string CurrentPassword { get; set; }
        public required string NewPassword { get; set; }
        public required string ConfirmPassword { get; set; }
        [JsonIgnore]
        public string Jti { get; set; } = string.Empty;
    }
}