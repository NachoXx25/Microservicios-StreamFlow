using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AuthMicroservice.src.Domain.Models;
namespace AuthMicroservice.src.Infrastructure.Data
{
    public class DataContext : IdentityDbContext<User, Role, int>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options){}

        public DbSet<TokenBlacklist> TokenBlacklists { get; set; } = null!;
    }

}