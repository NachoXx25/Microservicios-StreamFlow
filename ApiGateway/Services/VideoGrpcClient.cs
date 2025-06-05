using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiGateway.Protos.VideoService;
using Grpc.Net.Client;

namespace ApiGateway.Services
{
    public class VideoGrpcClient
    {
        private readonly GrpcChannel _channel;

        private readonly VideoGrpcService.VideoGrpcServiceClient _client;

        public VideoGrpcClient(IConfiguration configuration)
        {
            var videoServiceUrl = configuration["GrpcServices:VideoService"] ?? "http://localhost:5025/";
            _channel = GrpcChannel.ForAddress(videoServiceUrl);
            _client = new VideoGrpcService.VideoGrpcServiceClient(_channel);
        }

        public async Task<GetAllVideosResponse> GetAllVideosAsync(GetAllVideosRequest request)
        {
            try
            {
                return await _client.GetAllVideosAsync(request);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error obteniendo todos los videos: {ex.Message}", ex);
            }
        }

        public async Task<GetVideoByIdResponse> GetVideoByIdAsync(GetVideoByIdRequest request)
        {
            try
            {
                return await _client.GetVideoByIdAsync(request);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error obteniendo el video con ID {request.Id}: {ex.Message}", ex);
            }
        }

        public async Task<Video> CreateVideoAsync(UploadVideoRequest request)
        {
            try
            {
                return await _client.UploadVideoAsync(request);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creando el video: {ex.Message}", ex);
            }
        }

        public async Task<DeleteVideoResponse> DeleteVideoAsync(DeleteVideoRequest request)
        {
            try
            {
                return await _client.DeleteVideoAsync(request);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error eliminando el video con ID {request.Id}: {ex.Message}", ex);
            }
        }
        
        public async Task<UpdateVideoResponse> UpdateVideoAsync(UpdateVideoRequest request)
        {
            try
            {
                return await _client.UpdateVideoAsync(request);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error actualizando el video con ID {request.Id}: {ex.Message}", ex);
            }
        }
    }
}