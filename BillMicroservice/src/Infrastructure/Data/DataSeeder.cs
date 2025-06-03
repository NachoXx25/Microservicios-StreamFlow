using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BillMicroservice.src.Domain.Models.Bill;
using BillMicroservice.src.Domain.Models.User;
using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace BillMicroservice.src.Infrastructure.Data
{
    public class DataSeeder
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var billContext = scope.ServiceProvider.GetRequiredService<BillContext>();

                var logger = scope.ServiceProvider.GetRequiredService<ILogger<DataSeeder>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
                try
                {
                    try {
                        await billContext.Database.MigrateAsync();
                    }
                        catch (MySqlException ex) when (ex.Message.Contains("already exists")) {
                            logger.LogWarning("Algunas tablas ya existen en la base de datos MySQL: {Message}", ex.Message);
                    }
                    var roles = new[] { "Administrador", "Cliente" };
                    foreach(var roleName in roles)
                    {
                        if (!await roleManager.RoleExistsAsync(roleName))
                        {
                            var role = new Role { Name = roleName, NormalizedName = roleName.ToUpper() };
                            await roleManager.CreateAsync(role);

                            var createdRole = await billContext.Roles.FindAsync(role.Id);
                            if(createdRole != null)
                            {
                                var existsInBillContext = await billContext.Roles.AnyAsync( r => r.Name == roleName);
                                if(!existsInBillContext)
                                {
                                    await billContext.Roles.AddAsync(new Role { Id = createdRole.Id, Name = roleName, NormalizedName = roleName.ToUpper() });
                                    await billContext.SaveChangesAsync();
                                }
                            }
                        }
                    }

                    var statuses = new[] { "Pendiente", "Pagado", "Vencido" };
                    
                    if(!await billContext.Statuses.AnyAsync())
                    {
                        foreach(var status in statuses)
                        {
                            billContext.Statuses.Add(new Status { Name = status });
                        }
                        await billContext.SaveChangesAsync();
                    }
                }
                catch(Exception ex)
                {
                    logger.LogError(ex, "Un error ha ocurrido mientras se cargaban los seeders");
                }
            }
        }
    }
}