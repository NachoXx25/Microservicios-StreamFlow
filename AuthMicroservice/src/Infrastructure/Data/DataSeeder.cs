using AuthMicroservice.src.Domain.Models;
using Bogus;
using Microsoft.EntityFrameworkCore;
using AuthMicroservice.src.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Serilog;
namespace AuthMicroservice.src.Infrastructure.Data
{
    public class DataSeeder
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var Context = scope.ServiceProvider.GetRequiredService<DataContext>();

                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
                try
                {   
                    await Context.Database.MigrateAsync();
                    var roles = new[] { "Administrador", "Cliente" };
                    if (!await Context.Roles.AnyAsync())
                    {
                        foreach (var roleName in roles)
                        {
                            var role = new Role { Name = roleName, NormalizedName = roleName.ToUpper() };
                            await roleManager.CreateAsync(role);
                        }
                    }
                }
                catch(Exception ex)
                {
                    Log.Error(ex, "Un error ha ocurrido mientras se cargaban los seeders");
                }
            }
        }
    }
}