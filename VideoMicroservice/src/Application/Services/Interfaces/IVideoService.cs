using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VideoMicroservice.src.Application.DTOs;

namespace VideoMicroservice.src.Application.Services.Interfaces
{
    public interface IVideoService
    {
        /// <summary>
        /// Método que permite subir un video a la base de datos
        /// </summary>
        /// <param name="video">El video a subir</param>
        /// <returns>El video subido</returns>
        Task<GetVideoDTO> UploadVideo(UploadVideoDTO video);

        /// <summary>
        /// Método que permite obtener un video por su id
        /// </summary>
        /// <param name="id">El id del video a obtener</param>
        /// <returns>El video buscado</returns>
        Task<GetVideoDTO?> GetVideoById(string id);

        /// <summary>
        /// Método que permite actualizar un video por su id
        /// </summary>
        /// <param name="id">El id del video a actualizar</param>
        /// <param name="updateVideo">Los valores a actualizar del video</param>
        /// <returns>El video actualizado</returns>
        Task<UpdateVideoDTO> UpdateVideo(string id, UpdateVideoDTO updateVideo);

        /// <summary>
        /// Método que permite eliminar lógicamente un video por su id
        /// </summary>
        /// <param name="id">El id del video a eliminar</param>
        /// <returns></returns>
        Task DeleteVideo(string id);

        /// <summary>
        /// Método que permite obtener todos los videos no eliminados de la base de datos
        /// </summary>
        /// <param name="search">Filtro opcional por género o título</param>
        /// <returns>Listado de todos los videos según el filtrado</returns>
        Task<GetVideoDTO[]?> GetAllVideos(VideoSearchDTO? search);
    }
}