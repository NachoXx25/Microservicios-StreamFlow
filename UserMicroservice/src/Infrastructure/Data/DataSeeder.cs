using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using UserMicroservice.Services;
using UserMicroservice.src.Domain;

namespace UserMicroservice.src.Infrastructure.Data
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
                    if(!await Context.Users.AnyAsync())
                    {
                        var customer = new User(){
                            FirstName = "Juan",
                            LastName = "Perez",
                            UserName = Guid.NewGuid().ToString(),
                            NormalizedUserName = "JUANPEREZ",
                            Email = "juan@gmail.com",
                            NormalizedEmail = "JUAN@GMAIL.COM",
                            Status = true,
                            RoleId = Context.Roles.First(r => r.Name == "Cliente").Id
                        };
                        customer.PasswordHash = new PasswordHasher<User>().HashPassword(customer, "Password123!");
                        Context.Users.Add(customer);
                        var administrador = new User(){
                            
                            FirstName = "Juana",
                            LastName = "Valencia",
                            UserName = Guid.NewGuid().ToString(),
                            NormalizedUserName = "JUANAVALENCIA",
                            Email = "juana@gmail.com",
                            NormalizedEmail = "JUANA@GMAIL.COM",
                            Status = true,
                            RoleId = Context.Roles.First(r => r.Name == "Administrador").Id
                        };
                        administrador.PasswordHash = new PasswordHasher<User>().HashPassword(administrador, "Password123!");
                        Context.Users.Add(administrador);
                        var faker = new Faker<User>()
                            .RuleFor(u => u.UserName, f => Guid.NewGuid().ToString())
                            .RuleFor(u => u.NormalizedUserName, (f, u) => u.UserName?.ToUpper())
                            .RuleFor(u => u.Email, f => f.Internet.Email())
                            .RuleFor(u => u.NormalizedEmail, (f, u) => u.Email?.ToUpper())
                            .RuleFor(u => u.PasswordHash, (f, u) => new PasswordHasher<User>().HashPassword(u, "Password123!"))
                            .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                            .RuleFor(u => u.LastName, f => f.Name.LastName())
                            .RuleFor(u => u.Status, f => f.PickRandom(new[] { true, false }))
                            .RuleFor(u => u.RoleId, f => Context.Roles.First(r => r.Name == f.PickRandom(roles)).Id);
                        Context.Users.AddRange(faker.Generate(150));
                        await Context.SaveChangesAsync();
                        var userEventService = scope.ServiceProvider.GetRequiredService<IUserEventService>();
                        foreach (var user in Context.Users)
                        {
                            await userEventService.PublishUserCreatedEvent(user);
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