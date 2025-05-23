using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Http.HttpResults;
using VideoMicroservice.src.Application.Services.Interfaces;
using VideoMicroservice.src.Domain;

namespace VideoMicroservice.Services
{
    public class VideoGrpcService : Protos.VideoGrpcService.VideoGrpcServiceBase
    {
        
        private readonly IVideoService _videoService;

        public VideoGrpcService(IVideoService videoService)
        {
            _videoService = videoService;
        }

        public async override Task<Protos.Video> UploadVideo(Protos.UploadVideoRequest request, ServerCallContext context)
        {
            try
            {
                var video = new src.Application.DTOs.UploadVideoDTO
                {
                    Title = request.Title,
                    Description = request.Description,
                    Genre = request.Genre,
                };

                var createdVideo = await _videoService.UploadVideo(video);

                var response = new Protos.Video
                {
                    Id = createdVideo.Id.ToString(),
                    Title = createdVideo.Title,
                    Description = createdVideo.Description,
                    Likes = 0,
                    Genre = createdVideo.Genre,
                    IsDeleted = false
                };

                return response;
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public async override Task<Protos.GetVideoByIdResponse> GetVideoById(Protos.GetVideoByIdRequest request, ServerCallContext context)
        {
            try
            {
                var video = await _videoService.GetVideoById(request.Id);

                if (video == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, "Video not found"));
                }

                var response = new Protos.GetVideoByIdResponse
                {
                    Id = video.Id,
                    Title = video.Title,
                    Description = video.Description,
                    Likes = 0, //TODO: Obtain likes from the social interactions service
                    Genre = video.Genre,
                };

                return response;
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public async override Task<Protos.UpdateVideoResponse> UpdateVideo(Protos.UpdateVideoRequest request, ServerCallContext context)
        {
            try
            {
                var video = new src.Application.DTOs.UpdateVideoDTO
                {
                    Title = request.Title,
                    Description = request.Description,
                    Genre = request.Genre,
                };

                var updatedVideo = await _videoService.UpdateVideo(request.Id, video);

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
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public async override Task<Protos.DeleteVideoResponse> DeleteVideo(Protos.DeleteVideoRequest request, ServerCallContext context)
        {
            try
            {
                await _videoService.DeleteVideo(request.Id);
                return new Protos.DeleteVideoResponse();
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public async override Task<Protos.GetAllVideosResponse> GetAllVideos(Protos.GetAllVideosRequest request, ServerCallContext context)
        {
            try
            {

                var search = new src.Application.DTOs.VideoSearchDTO
                {
                    Title = request.Title,
                    Genre = request.Genre
                };

                var videos = await _videoService.GetAllVideos(search);

                var response = videos.Select(video => new Protos.Video
                {
                    Id = video.Id,
                    Title = video.Title,
                    Description = video.Description,
                    Genre = video.Genre
                }).ToList();

                var getAllVideosResponse = new Protos.GetAllVideosResponse
                {
                    Videos = { response }
                };

                return getAllVideosResponse;
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }
    }
}