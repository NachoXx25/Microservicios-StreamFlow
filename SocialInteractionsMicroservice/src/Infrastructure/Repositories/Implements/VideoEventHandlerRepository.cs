using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using SocialInteractionsMicroservice.src.Domain.Models;
using SocialInteractionsMicroservice.src.Infrastructure.Data;
using SocialInteractionsMicroservice.src.Infrastructure.MessageBroker.Models;
using SocialInteractionsMicroservice.src.Infrastructure.Repositories.Interfaces;

namespace SocialInteractionsMicroservice.src.Infrastructure.Repositories.Implements
{
    public class VideoEventHandlerRepository : IVideoEventHandlerRepository
    {
        private readonly SocialInteractionsContext _context;

        public VideoEventHandlerRepository(SocialInteractionsContext context)
        {
            _context = context;
        }

        public async Task HandleVideoCreatedEventAsync(VideoCreatedEvent videoCreatedEvent)
        {

            try
            {

                Log.Information("Video creado: {@VideoCreatedEvent}", videoCreatedEvent);

                var existingVideo = await _context.Videos.FindAsync(videoCreatedEvent.Id);

                if (existingVideo != null)
                {

                    throw new Exception("El video ya existe en la base de datos.");
                }

                var bsonId = new MongoDB.Bson.ObjectId(videoCreatedEvent.Id);

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

                var existingVideo = await _context.Videos.FindAsync(videoDeletedEvent.Id) ?? throw new Exception("El video no existe en la base de datos.");

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

                var existingVideo = await _context.Videos.FindAsync(videoUpdatedEvent.Id) ?? throw new Exception("El video no existe en la base de datos.");

                existingVideo.Title = videoUpdatedEvent.Title;
                existingVideo.Description = videoUpdatedEvent.Description;
                existingVideo.Genre = videoUpdatedEvent.Genre;

                await _context.SaveChangesAsync();
                Log.Information("Usuario actualizado con ID {VideoId}", videoUpdatedEvent.Id);
            }
            catch (Exception)
            {
                Log.Error("Error al manejar el evento de actualización de video: {@VideoUpdatedEvent}", videoUpdatedEvent);
                throw;
            }
        }
    }
}