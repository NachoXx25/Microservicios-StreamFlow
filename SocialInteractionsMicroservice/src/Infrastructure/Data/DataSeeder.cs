using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Microsoft.EntityFrameworkCore;
using SocialInteractionsMicroservice.src.Domain.Models;

namespace SocialInteractionsMicroservice.src.Infrastructure.Data
{
    public class DataSeeder
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var videoContext = scope.ServiceProvider.GetRequiredService<SocialInteractionsContext>();

                var logger = scope.ServiceProvider.GetRequiredService<ILogger<DataSeeder>>();

                try
                {
                    if (!await videoContext.Videos.AnyAsync())
                    {
                        var faker = new Faker<Video>()
                            .RuleFor(v => v.Title, f => f.Lorem.Sentence(3))
                            .RuleFor(v => v.Description, f => f.Lorem.Paragraph(2))
                            .RuleFor(v => v.Genre, f => f.PickRandom(new[] { "Acción", "Comedia", "Drama", "Terror", "Ciencia Ficción" }));
                        videoContext.Videos.AddRange(faker.Generate(450));
                        await videoContext.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Un error ha ocurrido mientras se cargaban los seeders");
                }

                try
                {
                    if (!await videoContext.VideoInteractions.AnyAsync())
                    {
                        var videoIds = await videoContext.Videos
                                                .Select(v => v.Id)
                                                .ToListAsync();
            
                        if (videoIds.Any())
                        {
                            var faker = new Faker<VideoInteractions>()
                                .RuleFor(v => v.VideoId, f => f.PickRandom(videoIds))
                                .RuleFor(v => v.Likes, f => 2)
                                .RuleFor(v => v.Comments, f => new List<string> { f.Lorem.Sentence() });
                                
                            // Generate and add interactions
                            videoContext.VideoInteractions.AddRange(faker.Generate(40));
                            await videoContext.SaveChangesAsync();
                        }
                        else
                        {
                            logger.LogWarning("No se encontraron videos para generar interacciones.");
                        }
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