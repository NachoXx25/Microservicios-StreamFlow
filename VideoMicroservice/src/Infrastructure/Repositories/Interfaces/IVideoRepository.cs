using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VideoMicroservice.src.Application.DTOs;
using VideoMicroservice.src.Domain;

namespace VideoMicroservice.src.Infrastructure.Repositories.Interfaces
{
    public interface IVideoRepository
    {
        /// <summary>
        /// Sube un video a la base de datos
        /// </summary>
        /// <param name="video">EL video a subir</param>
        /// <returns>El video subido</returns>
        Task<Video> UploadVideo(Video video);

        /// <summary>
        /// Obtiene un video por su id
        /// </summary>
        /// <param name="id">El id del video a obtener</param>
        /// <returns>El video solicitado</returns>
        Task<Video?> GetVideoById(string id);

        /// <summary>
        /// Actualiza un video en la base de datos
        /// </summary>
        /// <param name="id">El id del video a actualizar</param>
        /// <param name="updateVideo">Los datos a actualizar del video</param>
        /// <returns><Los datos actualizados del video/returns>
        Task<Video> UpdateVideo(string id, UpdateVideoDTO updateVideo);

        /// <summary>
        /// Hace un borrado lógico de un video
        /// </summary>
        /// <param name="id">El id del video a eliminar</param>
        Task DeleteVideo(string id);

        /// <summary>
        /// Obtiene todos los videos de la base de datos
        /// </summary>
        /// <returns>Listado de todos los videos</returns>
        Task<Video[]> GetAllVideos(VideoSearchDTO? search);
    }
}