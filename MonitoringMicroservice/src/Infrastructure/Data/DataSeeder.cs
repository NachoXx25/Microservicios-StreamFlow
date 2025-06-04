using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Microsoft.EntityFrameworkCore;
using MonitoringMicroservice.src.Domain.Models;

namespace MonitoringMicroservice.src.Infrastructure.Data
{
    public class DataSeeder
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var monitoringContext = scope.ServiceProvider.GetRequiredService<MonitoringContext>();

                var logger = scope.ServiceProvider.GetRequiredService<ILogger<DataSeeder>>();

                try
                {
                    if (!await monitoringContext.Actions.AnyAsync())
                    {
                        var faker = new Faker<Domain.Models.Action>()
                            .RuleFor(a => a.UserId, f => f.Random.Int(1, 100).ToString())
                            .RuleFor(a => a.UserEmail, f => f.Internet.Email())
                            .RuleFor(a => a.MethodUrl, f => f.Internet.Url())
                            .RuleFor(a => a.Timestamp, f => f.Date.Recent())
                            .RuleFor(a => a.Name, f => f.PickRandom(new List<string> { "CREAR VIDEO", "ELIMINAR FACTURA", "ACTUALIZAR USUARIO"}));
                        monitoringContext.Actions.AddRange(faker.Generate(10));
                        await monitoringContext.SaveChangesAsync();
                    }

                    if (!await monitoringContext.Errors.AnyAsync())
                    {
                        var faker = new Faker<Error>()
                            .RuleFor(e => e.Message, f => f.Lorem.Sentence())
                            .RuleFor(e => e.UserId, f => f.Random.Int(1, 100).ToString())
                            .RuleFor(e => e.UserEmail, f => f.Internet.Email())
                            .RuleFor(e => e.Timestamp, f => f.Date.Recent());
                        monitoringContext.Errors.AddRange(faker.Generate(10));
                        await monitoringContext.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Un error ha ocurrido mientras se cargaban los seeders");
                }
            }
        }
    }
}