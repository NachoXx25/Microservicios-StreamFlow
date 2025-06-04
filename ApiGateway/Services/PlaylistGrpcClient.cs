using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiGateway.Services;
using ApiGateway.Protos.PlaylistService;
using Grpc.Net.Client;
namespace ApiGateway.Services
{
    public class PlaylistGrpcClient
    {
        private readonly GrpcChannel _channel;
        private readonly PlaylistService.PlaylistServiceClient _client;

        public PlaylistGrpcClient(IConfiguration configuration)
        {
            var playlistServiceUrl = configuration["GrpcServices:PlaylistService"] ?? "http://localhost:5250/";
            _channel = GrpcChannel.ForAddress(playlistServiceUrl);
            _client = new PlaylistService.PlaylistServiceClient(_channel);
        }
        public async Task<GetPlaylistsByUserIdResponse> GetPlaylistsByUserIdAsync(GetPlaylistsByUserIdRequest request)
        {
            return await _client.GetPlaylistsByUserIdAsync(request);
        }
        public async Task<PlaylistCreatedResponse> CreatePlaylistAsync(CreatePlaylistRequest request)
        {
            return await _client.CreatePlaylistAsync(request);
        }
        public async Task<AddVideoToPlaylistResponse> AddVideoToPlaylistAsync(AddVideoToPlaylistRequest request)
        {
            return await _client.AddVideoToPlaylistAsync(request);
        }
        public async Task<GetVideosByPlaylistIdResponse> GetVideosByPlaylistIdAsync(GetVideosByPlaylistIdRequest request)
        {
            return await _client.GetVideosByPlaylistIdAsync(request);
        }
        public async Task<RemoveVideoFromPlaylistResponse> RemoveVideoFromPlaylistAsync(RemoveVideoFromPlaylistRequest request)
        {
            return await _client.RemoveVideoFromPlaylistAsync(request);
        }
        public async Task<DeletePlaylistResponse> DeletePlaylistAsync(DeletePlaylistRequest request)
        {
            return await _client.DeletePlaylistAsync(request);
        }
    }
}