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

        public async Task<GetAllVideosResponse> GetAllVideosAsync()
        {
            return await _client.GetAllVideosAsync(new GetAllVideosRequest());
        }

        public async Task<GetVideoByIdResponse> GetVideoByIdAsync(string videoId)
        {
            return await _client.GetVideoByIdAsync(new GetVideoByIdRequest { Id = videoId });
        }

        public async Task<Video> UploadVideoAsync(UploadVideoRequest request)
        {
            return await _client.UploadVideoAsync(request);
        }

        public async Task<UpdateVideoResponse> UpdateVideoAsync(UpdateVideoRequest request)
        {
            return await _client.UpdateVideoAsync(request);
        }

        public async Task<DeleteVideoResponse> DeleteVideoAsync(DeleteVideoRequest request)
        {
            return await _client.DeleteVideoAsync(request);
        }

        public void Dispose()
        {
            _channel?.Dispose();
        }
    }
}