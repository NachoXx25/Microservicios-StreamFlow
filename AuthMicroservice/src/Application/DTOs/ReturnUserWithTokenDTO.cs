namespace AuthMicroservice.src.Application.DTOs
{
    public class ReturnUserWithTokenDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; } 
        public bool IsActive { get; set; } = true;
        public string? Token { get; set; }
    }
}