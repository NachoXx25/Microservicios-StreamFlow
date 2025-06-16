namespace AuthMicroservice.src.Domain.Models
{
    public class TokenBlacklist
    {
        public int Id { get; set; }
        public string Jti { get; set; } = string.Empty;
        public DateTime RevokedAt { get; set; } = DateTime.UtcNow;
    }
}