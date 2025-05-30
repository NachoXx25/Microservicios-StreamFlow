using PlaylistMicroservice.src.Application.Services.Interfaces;
using PlaylistMicroservice.Protos;
using PlaylistMicroservice.src.Application.DTOs;
using Grpc.Core;
using Serilog;

namespace PlaylistMicroservice.Services
{
    public class PlaylistGrpcService : PlaylistService.PlaylistServiceBase
    {
        private readonly IPlaylistService _playlistService;

        public PlaylistGrpcService(IPlaylistService playlistService)
        {
            _playlistService = playlistService;
        }

        public override async Task<GetPlaylistsByUserIdResponse> GetPlaylistsByUserId(GetPlaylistsByUserIdRequest request, ServerCallContext context)
        {
            Log.Information("Obteniendo listas de reproducción para el usuario {UserId}", request.UserId);
            try
            {
                int.TryParse(request.UserId, out int userId);
                var playlists = await _playlistService.GetPlaylistsByUserId(userId);
                return new GetPlaylistsByUserIdResponse
                {
                    Playlists = { playlists.Select(p => new Playlist
                    {
                        Id = p.Id.ToString(),
                        Name = p.Name
                    }) }
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener las listas de reproducción para el usuario {UserId}", request.UserId);
                throw new RpcException(new Status(StatusCode.Internal, $"Error al obtener las listas de reproducción: {ex.Message}"));
            }
        }

        public override async Task<PlaylistCreatedResponse> CreatePlaylist(CreatePlaylistRequest request, ServerCallContext context)
        {
            Log.Information("Creando lista de reproducción para el usuario {UserId}", request.UserId);
            try
            {
                int.TryParse(request.UserId, out int userId);
                var playlist = await _playlistService.CreatePlaylist(request.Name, userId);
                return new PlaylistCreatedResponse
                {
                    Id = playlist.Id.ToString(),
                    Name = playlist.PlaylistName,
                    IsDeleted = playlist.IsDeleted,
                    UserId = playlist.UserId.ToString()
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al crear la lista de reproducción para el usuario {UserId}", request.UserId);
                throw new RpcException(new Status(StatusCode.Internal, $"Error al crear la lista de reproducción: {ex.Message}"));
            }
        }

        public override async Task<AddVideoToPlaylistResponse> AddVideoToPlaylist(AddVideoToPlaylistRequest request, ServerCallContext context)
        {
            Log.Information("Agregando video a la lista de reproducción {PlaylistId} para el usuario {UserId}", request.PlaylistId, request.UserId);
            try
            {
                int.TryParse(request.UserId, out int userId);
                int.TryParse(request.PlaylistId, out int playlistId);
                var playlist = await _playlistService.AddVideoToPlaylist(playlistId, request.VideoId, userId);
                return new AddVideoToPlaylistResponse
                {
                    PlaylistId = playlist.Id.ToString(),
                    Name = playlist.Name,
                    Videos = { playlist.Videos.Select(v => new VideoByPlaylist
                    {
                        Id = v.VideoId,
                        Name = v.VideoName
                    }) }
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al agregar el video a la lista de reproducción {PlaylistId} para el usuario {UserId}", request.PlaylistId, request.UserId);
                throw new RpcException(new Status(StatusCode.Internal, $"Error al agregar el video a la lista de reproducción: {ex.Message}"));
            }
        }

        public override async Task<GetVideosByPlaylistIdResponse> GetVideosByPlaylistId(GetVideosByPlaylistIdRequest request, ServerCallContext context)
        {
            Log.Information("Obteniendo videos de la lista de reproducción {PlaylistId} para el usuario {UserId}", request.PlaylistId, request.UserId);
            try
            {
                int.TryParse(request.UserId, out int userId);
                int.TryParse(request.PlaylistId, out int playlistId);
                var videos = await _playlistService.GetVideosByPlaylistId(playlistId, userId);
                return new GetVideosByPlaylistIdResponse
                {
                    Videos = { videos.Select(v => new VideoByPlaylist
                    {
                        Id = v.VideoId,
                        Name = v.VideoName
                    }) }
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener los videos de la lista de reproducción {PlaylistId} para el usuario {UserId}", request.PlaylistId, request.UserId);
                throw new RpcException(new Status(StatusCode.Internal, $"Error al obtener los videos de la lista de reproducción: {ex.Message}"));
            }
        }

        public override async Task<RemoveVideoFromPlaylistResponse> RemoveVideoFromPlaylist(RemoveVideoFromPlaylistRequest request, ServerCallContext context)
        {
            Log.Information("Eliminando video de la lista de reproducción {PlaylistId} para el usuario {UserId}", request.PlaylistId, request.UserId);
            try
            {
                int.TryParse(request.UserId, out int userId);
                int.TryParse(request.PlaylistId, out int playlistId);
                var playlist = await _playlistService.RemoveVideoFromPlaylist(playlistId, request.VideoId, userId);
                return new RemoveVideoFromPlaylistResponse
                {
                    Playlist = new AddVideoToPlaylistResponse
                    {
                        PlaylistId = playlist.Id.ToString(),
                        Name = playlist.Name,
                        Videos = { playlist.Videos.Select(v => new VideoByPlaylist
                        {
                            Id = v.VideoId,
                            Name = v.VideoName
                        }) }
                    }
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al eliminar el video de la lista de reproducción {PlaylistId} para el usuario {UserId}", request.PlaylistId, request.UserId);
                throw new RpcException(new Status(StatusCode.Internal, $"Error al eliminar el video de la lista de reproducción: {ex.Message}"));
            }
        }

        public override async Task<DeletePlaylistResponse> DeletePlaylist(DeletePlaylistRequest request, ServerCallContext context)
        {
            Log.Information("Eliminando lista de reproducción {PlaylistId} para el usuario {UserId}", request.PlaylistId, request.UserId);
            try
            {
                int.TryParse(request.UserId, out int userId);
                int.TryParse(request.PlaylistId, out int playlistId);
                var message = await _playlistService.DeletePlaylist(playlistId, userId);
                return new DeletePlaylistResponse { Response = message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al eliminar la lista de reproducción {PlaylistId} para el usuario {UserId}", request.PlaylistId, request.UserId);
                throw new RpcException(new Status(StatusCode.Internal, $"Error al eliminar la lista de reproducción: {ex.Message}"));
            }
        }
    }
}