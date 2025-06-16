using Bogus;
using Bogus.Bson;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using VideoMicroservice.Services;
using VideoMicroservice.src.Domain;

namespace VideoMicroservice.src.Infrastructure.Data
{
    public class DataSeeder
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var videoContext = scope.ServiceProvider.GetRequiredService<VideoContext>();

                var logger = scope.ServiceProvider.GetRequiredService<ILogger<DataSeeder>>();
                
                var videoEventPublisher = scope.ServiceProvider.GetRequiredService<IVideoEventService>();

                try
                {
                    if (!await videoContext.Videos.AnyAsync())
                    {
                        // Add some specific videos for testing purposes
                        var firstTestVideoId = new ObjectId("507f1f77bcf86cd799439011");

                        var firstTestVideo = new Video
                        {
                            Id = firstTestVideoId,
                            Title = "Primer Video de Prueba",
                            Description = "Este es un video de prueba para verificar la funcionalidad del microservicio de videos.",
                            Genre = "Comedia",
                            IsDeleted = false,
                            Likes = 0
                        };

                        var secondTestVideoid = new ObjectId("507f1f77bcf86cd799439012");

                        var secondTestVideo = new Video
                        {
                            Id = secondTestVideoid,
                            Title = "Segundo Video de Prueba",
                            Description = "Este es otro video de prueba para verificar la funcionalidad del microservicio de videos.",
                            Genre = "Acción",
                            IsDeleted = false,
                            Likes = 0
                        };

                        videoContext.Videos.AddRange(firstTestVideo, secondTestVideo);
                        await videoContext.SaveChangesAsync();

                        // Generate random videos using Bogus
                        var faker = new Faker<Video>()
                            .RuleFor(v => v.Title, f => f.Lorem.Sentence(3))
                            .RuleFor(v => v.Description, f => f.Lorem.Paragraph(2))
                            .RuleFor(v => v.Genre, f => f.PickRandom(new[] { "Acción", "Comedia", "Drama", "Terror", "Ciencia Ficción" }));
                        videoContext.Videos.AddRange(faker.Generate(450));
                        await videoContext.SaveChangesAsync();

                        var videos = await videoContext.Videos.ToListAsync();
                        foreach (var video in videos)
                        {
                            await videoEventPublisher.PublishCreatedVideo(video);
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