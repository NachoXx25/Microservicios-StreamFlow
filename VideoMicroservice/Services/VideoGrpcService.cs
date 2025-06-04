using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using VideoMicroservice.src.Application.Services.Interfaces;
using VideoMicroservice.src.Infrastructure.MessageBroker.Models;

namespace VideoMicroservice.Services
{
    public class VideoGrpcService : Protos.VideoGrpcService.VideoGrpcServiceBase
    {
        
        private readonly IVideoService _videoService;

        private readonly IMonitoringEventService _monitoringEventService;

        public VideoGrpcService(IVideoService videoService, IMonitoringEventService monitoringEventService)
        {
            _monitoringEventService = monitoringEventService;
            _videoService = videoService;
        }

        public async override Task<Protos.Video> UploadVideo(Protos.UploadVideoRequest request, ServerCallContext context)
        {
            try
            {
                await _monitoringEventService.PublishActionEventAsync(new ActionEvent
                {
                    ActionMessage = "Subir video",
                    Service = "VideoService",
                    UserId = request.UserData.Id,
                    UserEmail = request.UserData.Email,
                    UrlMethod = "POST/videos",
                });

                var video = new src.Application.DTOs.UploadVideoDTO
                {
                    Title = request.Title,
                    Description = request.Description,
                    Genre = request.Genre,
                };

                var createdVideo = await _videoService.UploadVideo(video);

                if (createdVideo == null)
                {
                    throw new Exception("Error al crear el video");
                }

                var response = new Protos.Video
                {
                    Id = createdVideo.Id.ToString(),
                    Title = createdVideo.Title,
                    Description = createdVideo.Description,
                    Likes = createdVideo.Likes,
                    Genre = createdVideo.Genre,
                    IsDeleted = false
                };

                return response;
            }
            catch (Exception ex)
            {
                await _monitoringEventService.PublishErrorEventAsync(new ErrorEvent
                {
                    ErrorMessage = $"Error al subir video: {ex.Message}",
                    Service = "VideoService",
                    UserId = request.UserData.Id,
                    UserEmail = request.UserData.Email,
                });
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public async override Task<Protos.GetVideoByIdResponse> GetVideoById(Protos.GetVideoByIdRequest request, ServerCallContext context)
        {
            try
            {
                await _monitoringEventService.PublishActionEventAsync(new ActionEvent
                {
                    ActionMessage = "Obtener video por ID",
                    Service = "VideoService",
                    UserId = request.UserData.Id,
                    UserEmail = request.UserData.Email,
                    UrlMethod = $"GET/videos/{request.Id}",
                });

                var video = await _videoService.GetVideoById(request.Id);

                if (video == null)
                {
                    throw new KeyNotFoundException($"Video con ID {request.Id} no encontrado");
                }

                var response = new Protos.GetVideoByIdResponse
                {
                    Id = video.Id,
                    Title = video.Title,
                    Description = video.Description,
                    Likes = video.Likes, 
                    Genre = video.Genre,
                };

                return response;
            }
            catch (KeyNotFoundException knfEx)
            {
                await _monitoringEventService.PublishErrorEventAsync(new ErrorEvent
                {
                    ErrorMessage = knfEx.Message,
                    Service = "VideoService",
                    UserId = request.UserData.Id,
                    UserEmail = request.UserData.Email,
                });
                throw new RpcException(new Status(StatusCode.NotFound, knfEx.Message));
            }
            catch (Exception ex)
            {
                await _monitoringEventService.PublishErrorEventAsync(new ErrorEvent
                {
                    ErrorMessage = $"Error al obtener video con id {request.Id}: {ex.Message}",
                    Service = "VideoService",
                    UserId = request.UserData.Id,
                    UserEmail = request.UserData.Email,
                });
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public async override Task<Protos.UpdateVideoResponse> UpdateVideo(Protos.UpdateVideoRequest request, ServerCallContext context)
        {
            try
            {
                await _monitoringEventService.PublishActionEventAsync(new ActionEvent
                {
                    ActionMessage = "Actualizar video",
                    Service = "VideoService",
                    UserId = request.UserData.Id,
                    UserEmail = request.UserData.Email,
                    UrlMethod = $"PATCH/videos/{request.Id}",
                });

                var video = new src.Application.DTOs.UpdateVideoDTO
                {
                    Title = request.Title,
                    Description = request.Description,
                    Genre = request.Genre,
                };

                var updatedVideo = await _videoService.UpdateVideo(request.Id, video);

                if (updatedVideo == null)
                {
                    throw new Exception("Error al actualizar el video");
                }

                var response = new Protos.UpdateVideoResponse
                {
                    Title = updatedVideo.Title,
                    Description = updatedVideo.Description,
                    Genre = updatedVideo.Genre
                };

                return response;
            }
            catch (Exception ex)
            {
                await _monitoringEventService.PublishErrorEventAsync(new ErrorEvent
                {
                    ErrorMessage = $"Error al actualizar video con id {request.Id}: {ex.Message}",
                    Service = "VideoService",
                    UserId = request.UserData.Id,
                    UserEmail = request.UserData.Email,
                });
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public async override Task<Protos.DeleteVideoResponse> DeleteVideo(Protos.DeleteVideoRequest request, ServerCallContext context)
        {
            try
            {
                await _monitoringEventService.PublishActionEventAsync(new ActionEvent
                {
                    ActionMessage = "Eliminar video",
                    Service = "VideoService",
                    UserId = request.UserData.Id,
                    UserEmail = request.UserData.Email,
                    UrlMethod = $"DELETE/videos/{request.Id}",
                });
                await _videoService.DeleteVideo(request.Id);
                return new Protos.DeleteVideoResponse();
            }
            catch (Exception ex)
            {
                await _monitoringEventService.PublishErrorEventAsync(new ErrorEvent
                {
                    ErrorMessage = $"Error al eliminar video con id {request.Id}: {ex.Message}",
                    Service = "VideoService",
                    UserId = request.UserData.Id,
                    UserEmail = request.UserData.Email,
                });
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public async override Task<Protos.GetAllVideosResponse> GetAllVideos(Protos.GetAllVideosRequest request, ServerCallContext context)
        {
            try
            {
                await _monitoringEventService.PublishActionEventAsync(new ActionEvent
                {
                    ActionMessage = "Obtener todos los videos",
                    Service = "VideoService",
                    UserId = request.UserData.Id,
                    UserEmail = request.UserData.Email,
                    UrlMethod = "GET/videos",
                });

                var search = new src.Application.DTOs.VideoSearchDTO
                {
                    Title = request.Title,
                    Genre = request.Genre
                };

                var videos = await _videoService.GetAllVideos(search);

                if (videos == null || !videos.Any())
                {
                    throw new KeyNotFoundException("No se encontraron videos con los criterios especificados.");
                }

                var response = videos.Select(video => new Protos.Video
                {
                    Id = video.Id,
                    Title = video.Title,
                    Description = video.Description,
                    Genre = video.Genre,
                    Likes = video.Likes,
                }).ToList();

                var getAllVideosResponse = new Protos.GetAllVideosResponse
                {
                    Videos = { response }
                };

                return getAllVideosResponse;
            }
            catch (KeyNotFoundException knfEx)
            {
                await _monitoringEventService.PublishErrorEventAsync(new ErrorEvent
                {
                    ErrorMessage = knfEx.Message,
                    Service = "VideoService",
                    UserId = request.UserData.Id,
                    UserEmail = request.UserData.Email,
                });
                throw new RpcException(new Status(StatusCode.NotFound, knfEx.Message));
            }
            catch (Exception ex)
            {
                await _monitoringEventService.PublishErrorEventAsync(new ErrorEvent
                {
                    ErrorMessage = $"Error al obtener todos los videos: {ex.Message}",
                    Service = "VideoService",
                    UserId = request.UserData.Id,
                    UserEmail = request.UserData.Email,
                });
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }
    }
}