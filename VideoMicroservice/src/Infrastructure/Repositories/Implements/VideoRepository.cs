using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VideoMicroservice.src.Application.DTOs;
using VideoMicroservice.src.Domain;
using VideoMicroservice.src.Infrastructure.Data;
using VideoMicroservice.src.Infrastructure.Repositories.Interfaces;

namespace VideoMicroservice.src.Infrastructure.Repositories.Implements
{
    public class VideoRepository : IVideoRepository
    {
        private readonly VideoContext _context;

        public VideoRepository(VideoContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Hace un borrado lógico de un video
        /// </summary>
        /// <param name="id">El id del video a eliminar</param>
        /// <returns></returns>
        public async Task DeleteVideo(string id)
        {
            //Buscar el video por su id
            var video = await _context.Videos.FirstOrDefaultAsync(v => v.Id.ToString() == id) ?? throw new Exception("Video no encontrado");

            //Cambiar el estado del video a eliminado
            video.IsDeleted = true;

            //Guardar los cambios en la base de datos
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Obtiene todos los videos no eliminados de la base de datos
        /// </summary>
        /// <param name="search">Filtro opcional por género, título o ambos al mismo tiempo</param>
        /// <returns>Listado de todos los videos</returns>
        public async Task<Video[]> GetAllVideos(VideoSearchDTO? search)
        {
            //Obtenemos todos los videos no eliminados de la base de datos
            var videos = _context.Videos.AsQueryable();
            videos = videos.Where(v => !v.IsDeleted);

            //Filtramos por genero o titulo si se proporciona el filtro
            if(!string.IsNullOrWhiteSpace(search?.Title))
            {
                videos = videos.Where(v => v.Title.ToLower().Contains(search.Title.ToLower()));
            }
            if(!string.IsNullOrWhiteSpace(search?.Genre))
            {
                videos = videos.Where(v => v.Genre.ToLower().Contains(search.Genre.ToLower()));
            }
            
            //Retornamos los videos filtrados
            return await videos.ToArrayAsync();
        }

        /// <summary>
        /// Obtiene un video por su id
        /// </summary>
        /// <param name="id">El id del video a obtener</param>
        /// <returns>El video solicitado</returns>
        public async Task<Video?> GetVideoById(string id)
        {
            //Buscar el video por su id
            return await _context.Videos.AsNoTracking().FirstOrDefaultAsync(v => v.Id.ToString() == id);
        }

        /// <summary>
        /// Actualiza un video en la base de datos
        /// </summary>
        /// <param name="id">El id del video a actualizar</param>
        /// <param name="updateVideo">Los datos a actualizar del video</param>
        /// <returns><Los datos actualizados del video/returns>
        public async Task<Video> UpdateVideo(string id, UpdateVideoDTO updateVideo)
        {
            //Buscar el video por su id
            var video = await _context.Videos.FirstOrDefaultAsync(v => v.Id.ToString() == id) ?? throw new Exception("Video no encontrado");

            //Si titulo, descripcion o genero no son nulos o vacios y son diferentes a los del video, se actualizan
            if(!string.IsNullOrWhiteSpace(updateVideo.Title) && updateVideo.Title != video.Title)
            {
                video.Title = updateVideo.Title;
            }
            if(!string.IsNullOrWhiteSpace(updateVideo.Description) && updateVideo.Description != video.Description)
            {
                video.Description = updateVideo.Description;
            }
            if(!string.IsNullOrWhiteSpace(updateVideo.Genre) && updateVideo.Genre != video.Genre)
            {
                video.Genre = updateVideo.Genre;
            }

            //Guardar los cambios en la base de datos
            await _context.SaveChangesAsync();

            //Retornar el video actualizado
            return video;
        }

        /// <summary>
        /// Sube un video a la base de datos
        /// </summary>
        /// <param name="video">EL video a subir</param>
        /// <returns>El video subido</returns>
        public async Task<Video> UploadVideo(Video video)
        {
            await _context.Videos.AddAsync(video);
            await _context.SaveChangesAsync();
            return video;
        }
    }
}