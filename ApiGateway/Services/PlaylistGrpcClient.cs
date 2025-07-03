using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiGateway.Services;
using ApiGateway.Protos.PlaylistService;
using Grpc.Net.Client;
using Grpc.Core;
using Serilog;
using DotNetEnv;
namespace ApiGateway.Services
{
    public class PlaylistGrpcClient
    {
        private readonly GrpcChannel _channel;
        private readonly PlaylistService.PlaylistServiceClient _client;

        public PlaylistGrpcClient(IConfiguration configuration)
        {
            var playlistServiceUrl = Env.GetString("GrpcServices__PlaylistService") ?? "http://localhost:5250/";
            _channel = GrpcChannel.ForAddress(playlistServiceUrl);
            _client = new PlaylistService.PlaylistServiceClient(_channel);
        }
        public async Task<GetPlaylistsByUserIdResponse> GetPlaylistsByUserIdAsync(GetPlaylistsByUserIdRequest request)
        {
            try
            {
                return await _client.GetPlaylistsByUserIdAsync(request);
            }
            catch (RpcException ex)
            {
                Log.Error(ex, "Error gRPC obteniendo playlists");
                throw new InvalidOperationException(ex.Status.Detail);
            }
        }
        public async Task<PlaylistCreatedResponse> CreatePlaylistAsync(CreatePlaylistRequest request)
        {
            try
            {
                return await _client.CreatePlaylistAsync(request);
            }
            catch (RpcException ex)
            {
                Log.Error(ex, "Error gRPC creando playlist");
                throw new InvalidOperationException(ex.Status.Detail);
            }
        }
        public async Task<AddVideoToPlaylistResponse> AddVideoToPlaylistAsync(AddVideoToPlaylistRequest request)
        {
            try
            {
                return await _client.AddVideoToPlaylistAsync(request);
            }
            catch (RpcException ex)
            {
                Log.Error(ex, "Error gRPC agregando video a playlist");
                throw new InvalidOperationException(ex.Status.Detail);
            }
        }
        public async Task<GetVideosByPlaylistIdResponse> GetVideosByPlaylistIdAsync(GetVideosByPlaylistIdRequest request)
        {
             try
            {
                return await _client.GetVideosByPlaylistIdAsync(request);
            }
            catch (RpcException ex)
            {
                Log.Error(ex, "Error gRPC obteniendo videos de playlist");
                throw new InvalidOperationException(ex.Status.Detail);
            }
        }
        public async Task<RemoveVideoFromPlaylistResponse> RemoveVideoFromPlaylistAsync(RemoveVideoFromPlaylistRequest request)
        {
            try
            {
                return await _client.RemoveVideoFromPlaylistAsync(request);
            }
            catch (RpcException ex)
            {
                Log.Error(ex, "Error gRPC removiendo video de playlist");
                throw new InvalidOperationException(ex.Status.Detail);
            }
        }
        public async Task<DeletePlaylistResponse> DeletePlaylistAsync(DeletePlaylistRequest request)
        {
            try
            {
                return await _client.DeletePlaylistAsync(request);
            }
            catch (RpcException ex)
            {
                Log.Error(ex, "Error gRPC eliminando playlist");
                throw new InvalidOperationException(ex.Status.Detail);
            }
        }
    }
}