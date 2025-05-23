using Bogus;
using Microsoft.EntityFrameworkCore;
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
                        var faker = new Faker<Video>()
                            .RuleFor(v => v.Title, f => f.Lorem.Sentence(3))
                            .RuleFor(v => v.Description, f => f.Lorem.Paragraph(2))
                            .RuleFor(v => v.Genre, f => f.PickRandom(new[] { "Acción", "Comedia", "Drama", "Terror", "Ciencia Ficción" }));
                        videoContext.Videos.AddRange(faker.Generate(450));
                        await videoContext.SaveChangesAsync();
                    }

                    var videos = await videoContext.Videos.ToListAsync();
                    foreach (var video in videos)
                    {
                        await videoEventPublisher.PublishCreatedVideo(video);
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