using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PlaylistMicroservice.src.Domain.Models;
using PlaylistMicroservice.src.Infrastructure.Data;
using PlaylistMicroservice.src.Infrastructure.MessageBroker.Models;
using PlaylistMicroservice.src.Infrastructure.Repositories.Interfaces;
using Serilog;

namespace PlaylistMicroservice.src.Infrastructure.Repositories.Implements
{
    public class VideoEventHandlerRepository : IVideoEventHandlerRepository
    {
        private readonly DataContext _context;

        public VideoEventHandlerRepository(DataContext context)
        {
            _context = context;
        }
        public async Task HandleVideoCreatedEvent(VideoCreated video)
        {
            try
            {
                var existingVideo = await _context.Videos.FirstOrDefaultAsync(v => v.Id == video.Id);
                if (existingVideo != null)
                {
                    Log.Warning("El video con ID {VideoId} ya existe.", video.Id);
                    return;
                }
                var newVideo = new Video
                {
                    Id = video.Id,
                    VideoName = video.Title,
                };
                await _context.AddAsync(newVideo);
                await _context.SaveChangesAsync();
                Log.Information("Video con ID {videoId} creado correctamente", video.Id);
            }
            catch (Exception ex)
            {
                Log.Error("Error al cargar el video creado", video, ex.Message);
                throw;
            }
        }

        public async Task HandleVideoDeletedEvent(VideoDeleted video)
        {
            try
            {
                var existingvideo = await _context.Videos.FirstOrDefaultAsync(v => v.Id == video.Id);
                if (existingvideo == null)
                {
                    Log.Warning("El video con el Id {video.Id} no existe", video.Id);
                    return;
                }
                existingvideo.IsDeleted = true;
                await _context.SaveChangesAsync();
                Log.Information("Video con ID {videoId} eliminado correctamente", video.Id);
            }
            catch (Exception ex)
            {
                Log.Error("Error al editar el video", video, ex.Message);
            }
        }

        public async Task HandleVideoUpdatedEvent(VideoUpdated video)
        {
            try
            {
                var existingvideo = await _context.Videos.FirstOrDefaultAsync(v => v.Id == video.Id);
                if (existingvideo == null)
                {
                    Log.Warning("El video con el Id {video.Id} no existe", video.Id);
                    return;
                }
                existingvideo.VideoName = video.Title;
                await _context.SaveChangesAsync();
                Log.Information("Video con ID {videoId} editado correctamente", video.Id);
            }
            catch (Exception ex)
            {
                Log.Error("Error al editar el video", video, ex.Message);
            }
        }

        
    }
}