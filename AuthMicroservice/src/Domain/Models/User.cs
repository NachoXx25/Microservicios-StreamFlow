using Microsoft.AspNetCore.Identity;
namespace AuthMicroservice.src.Domain.Models
{
    public class User : IdentityUser<int>
    {
        public required string FirstName { get; set; }  
        public required string LastName { get; set; }  
        public required int RoleId { get; set; }  
        public Role Role { get; set; }  = null!;
        public required bool Status { get; set; }  
        public DateTime CreatedAt { get; set; }  = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }  = DateTime.UtcNow;
    }
}