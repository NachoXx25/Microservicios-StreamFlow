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
                    var videoIds = await videoContext.Videos
                                                .Select(v => v.Id.ToString())
                                                .ToListAsync();

                    if (!await videoContext.Likes.AnyAsync())
                    {

                        if (videoIds.Any())
                        {
                            var faker = new Faker<Like>()
                                .RuleFor(l => l.VideoId, f => f.PickRandom(videoIds));

                            videoContext.Likes.AddRange(faker.Generate(75));
                            await videoContext.SaveChangesAsync();
                        }
                        else
                        {
                            logger.LogWarning("No se encontraron videos para generar interacciones.");
                        }
                    }

                    if (!await videoContext.Comments.AnyAsync())
                    {
                        if (videoIds.Any())
                        {
                            var faker = new Faker<Comment>()
                                .RuleFor(c => c.VideoId, f => f.PickRandom(videoIds))
                                .RuleFor(c => c.Content, f => f.Lorem.Sentence(10));

                            videoContext.Comments.AddRange(faker.Generate(35));
                            await videoContext.SaveChangesAsync();
                        }
                        else
                        {
                            logger.LogWarning("No se encontraron videos para generar comentarios.");
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