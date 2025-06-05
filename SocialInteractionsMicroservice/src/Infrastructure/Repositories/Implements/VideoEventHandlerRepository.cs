using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SocialInteractionsMicroservice.Services;
using SocialInteractionsMicroservice.src.Domain.Models;
using SocialInteractionsMicroservice.src.Infrastructure.Data;
using SocialInteractionsMicroservice.src.Infrastructure.MessageBroker.Models;
using SocialInteractionsMicroservice.src.Infrastructure.Repositories.Interfaces;

namespace SocialInteractionsMicroservice.src.Infrastructure.Repositories.Implements
{
    public class VideoEventHandlerRepository : IVideoEventHandlerRepository
    {
        private readonly SocialInteractionsContext _context;

        private readonly ISocialInteractionsEventService _socialInteractionsEventService;

        public VideoEventHandlerRepository(SocialInteractionsContext context, ISocialInteractionsEventService socialInteractionsEventService)
        {
            _context = context;
            _socialInteractionsEventService = socialInteractionsEventService;
        }

        public async Task HandleVideoCreatedEventAsync(VideoCreatedEvent videoCreatedEvent)
        {

            try
            {
                Log.Information("Video creado: {@VideoCreatedEvent}", videoCreatedEvent);

                var bsonId = new MongoDB.Bson.ObjectId(videoCreatedEvent.Id);

                var existingVideo = await _context.Videos.FindAsync(bsonId);

                if (existingVideo != null)
                {
                    Log.Warning("El video con ID {VideoId} ya existe en la base de datos.", videoCreatedEvent.Id);
                    return;
                }

                await _context.Videos.AddAsync(new Video
                {
                    Id = bsonId,
                    Title = videoCreatedEvent.Title,
                    Description = videoCreatedEvent.Description,
                    Genre = videoCreatedEvent.Genre,
                    IsDeleted = videoCreatedEvent.IsDeleted,
                });
                await _context.SaveChangesAsync();
                Log.Information("Video creado exitosamente: {@VideoCreatedEvent}", videoCreatedEvent);

                var videoCount = await _context.Videos.CountAsync();
                if (videoCount < 400 || videoCount > 600)
                {
                    Log.Information("No se cumplen las condiciones para ejecutar los seeders de interacciones sociales. Cantidad de videos: {VideoCount}", videoCount);
                    return;
                }
                else
                {
                    if (await _context.Likes.AnyAsync() || await _context.Comments.AnyAsync())
                    {
                        Log.Information("Ya existen interacciones sociales, omitiendo seeders.");
                        return;
                    }
                    await TriggerSeedersIfNeeded();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al manejar el evento de creación de video: {@VideoCreatedEvent}", videoCreatedEvent);
                throw;
            }
        }

        public async Task HandleVideoDeletedEventAsync(VideoDeletedEvent videoDeletedEvent)
        {
            try
            {
                Log.Information("Video eliminado: {@VideoDeletedEvent}", videoDeletedEvent);

                var bsonId = new MongoDB.Bson.ObjectId(videoDeletedEvent.Id);

                var existingVideo = await _context.Videos.FindAsync(bsonId) ?? throw new KeyNotFoundException("El video no existe en la base de datos.");

                existingVideo.IsDeleted = true;

                await _context.SaveChangesAsync();

                Log.Information("Video eliminado con ID {VideoId}", videoDeletedEvent.Id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al manejar el evento de eliminación de video: {@VideoDeletedEvent}", videoDeletedEvent);
                throw;
            }
        }

        public async Task HandleVideoUpdatedEventAsync(VideoUpdatedEvent videoUpdatedEvent)
        {
            try
            {
                Log.Information("Video actualizado: {@VideoUpdatedEvent}", videoUpdatedEvent);

                var bsonId = new MongoDB.Bson.ObjectId(videoUpdatedEvent.Id);

                var existingVideo = await _context.Videos.FindAsync(bsonId) ?? throw new KeyNotFoundException("El video no existe en la base de datos.");

                existingVideo.Title = videoUpdatedEvent.Title;
                existingVideo.Description = videoUpdatedEvent.Description;
                existingVideo.Genre = videoUpdatedEvent.Genre;

                await _context.SaveChangesAsync();
                Log.Information("Video actualizado con ID {VideoId}", videoUpdatedEvent.Id);
            }
            catch (Exception)
            {
                Log.Error("Error al manejar el evento de actualización de video: {@VideoUpdatedEvent}", videoUpdatedEvent);
                throw;
            }
        }
        
        private async Task TriggerSeedersIfNeeded()
        {
            try
            {
                var videoIds = await _context.Videos
                                        .Select(v => v.Id.ToString())
                                        .ToListAsync();

                if (!await _context.Likes.AnyAsync())
                {
                    Log.Information("Ejecutando seeders de likes...");
                    // Generar Likes
                    var fakerLikes = new Faker<Like>()
                        .RuleFor(l => l.VideoId, f => f.PickRandom(videoIds));

                    await _context.Likes.AddRangeAsync(fakerLikes.Generate(75));
                    await _context.SaveChangesAsync();
                    Log.Information("Seeders de Likes ejecutados exitosamente.");

                    var likes = await _context.Likes.AsNoTracking().ToListAsync();
                    foreach (var like in likes)
                    {
                        await _socialInteractionsEventService.PublishLikeEvent(like.VideoId.ToString());
                    }
                }
                else
                {
                    Log.Information("Ya existen likes, omitiendo seeder de likes");
                }

                if (!await _context.Comments.AnyAsync())
                {
                    Log.Information("Ejecutando seeders de comentarios...");
                    // Generar Comentarios
                    var fakerComments = new Faker<Comment>()
                        .RuleFor(c => c.VideoId, f => f.PickRandom(videoIds))
                        .RuleFor(c => c.Content, f => f.Lorem.Sentence(10));

                    await _context.Comments.AddRangeAsync(fakerComments.Generate(35));
                    await _context.SaveChangesAsync();
                    Log.Information("Seeders de Comentarios ejecutados exitosamente.");
                }
                else
                {
                    Log.Information("Ya existen comentarios, omitiendo seeder de comentarios");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error ejecutando seeders de interacciones");
            }
        }   
    }
}