using PlaylistMicroservice.src.Application.Services.Interfaces;
using PlaylistMicroservice.Protos;
using PlaylistMicroservice.src.Application.DTOs;
using Grpc.Core;
using Serilog;
using PlaylistMicroservice.src.Domain.Models;

namespace PlaylistMicroservice.Services
{
    public class PlaylistGrpcService : PlaylistService.PlaylistServiceBase
    {
        private readonly IPlaylistService _playlistService;
        private readonly IMonitoringEventService _monitoringEventService;

        public PlaylistGrpcService(IPlaylistService playlistService, IMonitoringEventService monitoringEventService)
        {
            _playlistService = playlistService;
            _monitoringEventService = monitoringEventService;
        }

        public override async Task<GetPlaylistsByUserIdResponse> GetPlaylistsByUserId(GetPlaylistsByUserIdRequest request, ServerCallContext context)
        {
            Log.Information("Obteniendo listas de reproducción para el usuario {UserId}", request.UserId);
            try
            {
                if(string.IsNullOrWhiteSpace(request.UserId)) throw new Exception("El id del usuario no puede estar vacío");
                if (!int.TryParse(request.UserId, out int userId)) throw new Exception("El id debe ser un número entero positivo");
                var playlists = await _playlistService.GetPlaylistsByUserId(userId);
                if(playlists.Count() == 0) 
                {
                    Log.Warning("No se encontraron listas de reproducción para el usuario {UserId}", userId);
                    throw new Exception($"No se encontraron listas de reproducción para el usuario {userId}");
                }
                await _monitoringEventService.PublishActionEventAsync(new ActionEvent
                {
                    ActionMessage = $"Obtener playlist por id de usuario",
                    UserId = userId.ToString(),
                    UserEmail = request.UserEmail,
                    UrlMethod = "GET/listas-reproduccion",
                    Service = "PlaylistService"
                });
                return new GetPlaylistsByUserIdResponse
                {
                    Playlists = { playlists.Select(p => new Protos.Playlist
                    {
                        Id = p.Id.ToString(),
                        Name = p.Name
                    }) }
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener las listas de reproducción para el usuario {UserId}", request.UserId);
                await _monitoringEventService.PublishErrorEventAsync(new ErrorEvent
                {
                    ErrorMessage = $"Error al obtener las listas de reproduccion del usuario {request.UserId}: {ex.Message}",
                    UserId = request.UserId,
                    UserEmail = request.UserEmail,
                    Service = "PlaylistService"
                });
                throw new RpcException(new Status(StatusCode.Internal, $"Error al obtener las listas de reproducción: {ex.Message}"));
            }
        }

        public override async Task<PlaylistCreatedResponse> CreatePlaylist(CreatePlaylistRequest request, ServerCallContext context)
        {
            Log.Information("Creando lista de reproducción para el usuario {UserId}", request.UserId);
            try
            {
                if (string.IsNullOrWhiteSpace(request.UserId)) throw new Exception("El id del usuario no puede estar vacío");
                if (!int.TryParse(request.UserId, out int userId)) throw new Exception("El id del usuario debe ser un número entero positivo");
                if (string.IsNullOrWhiteSpace(request.Name)) throw new Exception("El nombre de la lista de reproducción no puede estar vacío");
                var createdPlaylist = new CreatePlaylistDTO
                {
                    Name = request.Name
                };
                var playlist = await _playlistService.CreatePlaylist(createdPlaylist.Name, userId);
                await _monitoringEventService.PublishActionEventAsync(new ActionEvent
                {
                    ActionMessage = $"El usuario {userId} ha creado una nueva lista de reproduccion: {playlist.PlaylistName}",
                    UserId = userId.ToString(),
                    UserEmail = request.UserEmail,
                    UrlMethod = "POST/listas-reproduccion",
                    Service = "PlaylistService"
                });
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
                await _monitoringEventService.PublishErrorEventAsync(new ErrorEvent
                {
                    ErrorMessage = $"Error al crear la lista de reproduccion del usuario {request.UserId}: {ex.Message}",
                    UserId = request.UserId,
                    UserEmail = request.UserEmail,
                    Service = "PlaylistService"
                });
                throw new RpcException(new Status(StatusCode.Internal, $"Error al crear la lista de reproducción: {ex.Message}"));
            }
        }

        public override async Task<AddVideoToPlaylistResponse> AddVideoToPlaylist(AddVideoToPlaylistRequest request, ServerCallContext context)
        {
            Log.Information("Agregando video a la lista de reproducción {PlaylistId} para el usuario {UserId}", request.PlaylistId, request.UserId);
            try
            {
                if(string.IsNullOrWhiteSpace(request.VideoId)) throw new Exception("El id del video no puede estar vacío");
                if(string.IsNullOrWhiteSpace(request.PlaylistId)) throw new Exception("El id de la lista de reproducción no puede estar vacío");
                if(string.IsNullOrWhiteSpace(request.UserId)) throw new Exception("El id del usuario no puede estar vacío");
                if (!int.TryParse(request.UserId, out int userId)) throw new Exception("El id del usuario debe ser un número entero positivo");
                if(!int.TryParse(request.PlaylistId, out int playlistId)) throw new Exception("El id de la lista de reproducción debe ser un número entero positivo");
                var addVideoDTO = new AddVideoToPlaylistDTO
                {
                    VideoId = request.VideoId,
                    PlaylistId = request.PlaylistId
                };
                var playlist = await _playlistService.AddVideoToPlaylist(playlistId, request.VideoId, userId);
                await _monitoringEventService.PublishActionEventAsync(new ActionEvent
                {
                    ActionMessage = $"El usuario {userId} ha agregado el video {request.VideoId} a la lista de reproduccion {playlistId}",
                    UserId = userId.ToString(),
                    UserEmail = request.UserEmail,
                    UrlMethod = $"POST/listas-reproduccion/{playlistId}/videos",
                    Service = "PlaylistService"
                });
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
                await _monitoringEventService.PublishErrorEventAsync(new ErrorEvent
                {
                    ErrorMessage = $"Error al agregar el video {request.VideoId} a la lista de reproduccion {request.PlaylistId} del usuario {request.UserId}: {ex.Message}",
                    UserId = request.UserId,
                    UserEmail = request.UserEmail,
                    Service = "PlaylistService"
                });
                throw new RpcException(new Status(StatusCode.Internal, $"Error al agregar el video a la lista de reproducción: {ex.Message}"));
            }
        }

        public override async Task<GetVideosByPlaylistIdResponse> GetVideosByPlaylistId(GetVideosByPlaylistIdRequest request, ServerCallContext context)
        {
            Log.Information("Obteniendo videos de la lista de reproducción {PlaylistId} para el usuario {UserId}", request.PlaylistId, request.UserId);
            try
            {
                if(string.IsNullOrWhiteSpace(request.PlaylistId)) throw new Exception("El id de la lista de reproducción no puede estar vacío");
                if(string.IsNullOrWhiteSpace(request.UserId)) throw new Exception("El id del usuario no puede estar vacío");
                if (!int.TryParse(request.UserId, out int userId)) throw new Exception("El id debe ser un número entero positivo");
                if(!int.TryParse(request.PlaylistId, out int playlistId)) throw new Exception("El id de la lista de reproducción debe ser un número entero positivo");
                var videos = await _playlistService.GetVideosByPlaylistId(playlistId, userId);
                await _monitoringEventService.PublishActionEventAsync(new ActionEvent
                {
                    ActionMessage = $"El usuario {userId} ha solicitado los videos de la lista de reproduccion {playlistId}",
                    UserId = userId.ToString(),
                    UserEmail = request.UserEmail,
                    UrlMethod = $"GET/listas-reproduccion/{playlistId}/videos",
                    Service = "PlaylistService"
                });
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
                await _monitoringEventService.PublishErrorEventAsync(new ErrorEvent
                {
                    ErrorMessage = $"Error al obtener los videos de la lista de reproduccion {request.PlaylistId} del usuario {request.UserId}: {ex.Message}",
                    UserId = request.UserId,
                    UserEmail = request.UserEmail,
                    Service = "PlaylistService"
                });
                throw new RpcException(new Status(StatusCode.Internal, $"Error al obtener los videos de la lista de reproducción: {ex.Message}"));
            }
        }

        public override async Task<RemoveVideoFromPlaylistResponse> RemoveVideoFromPlaylist(RemoveVideoFromPlaylistRequest request, ServerCallContext context)
        {
            Log.Information("Eliminando video de la lista de reproducción {PlaylistId} para el usuario {UserId}", request.PlaylistId, request.UserId);
            try
            {
                if(string.IsNullOrWhiteSpace(request.VideoId)) throw new Exception("El id del video no puede estar vacío");
                if(string.IsNullOrWhiteSpace(request.PlaylistId)) throw new Exception("El id de la lista de reproducción no puede estar vacío");
                if (!int.TryParse(request.UserId, out int userId)) throw new Exception("El id debe ser un número entero positivo");
                if(!int.TryParse(request.PlaylistId, out int playlistId)) throw new Exception("El id de la lista de reproducción debe ser un número entero positivo");
                var removeVideoDTO = new RemoveVideoDTO
                {
                    VideoId = request.VideoId,
                    PlaylistId = request.PlaylistId
                };
                var playlist = await _playlistService.RemoveVideoFromPlaylist(playlistId, removeVideoDTO.VideoId, userId);
                await _monitoringEventService.PublishActionEventAsync(new ActionEvent
                {
                    ActionMessage = $"El usuario {userId} ha eliminado el video {removeVideoDTO.VideoId} de la lista de reproduccion {playlistId}",
                    UserId = userId.ToString(),
                    UserEmail = request.UserEmail,
                    UrlMethod = $"DELETE/listas-reproduccion/{playlistId}/videos",
                    Service = "PlaylistService"
                });
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
                await _monitoringEventService.PublishErrorEventAsync(new ErrorEvent
                {
                    ErrorMessage = $"Error al eliminar el video {request.VideoId} de la lista de reproduccion {request.PlaylistId} del usuario {request.UserId}: {ex.Message}",
                    UserId = request.UserId,
                    UserEmail = request.UserEmail,
                    Service = "PlaylistService"
                });
                throw new RpcException(new Status(StatusCode.Internal, $"Error al eliminar el video de la lista de reproducción: {ex.Message}"));
            }
        }

        public override async Task<DeletePlaylistResponse> DeletePlaylist(DeletePlaylistRequest request, ServerCallContext context)
        {
            Log.Information("Eliminando lista de reproducción {PlaylistId} para el usuario {UserId}", request.PlaylistId, request.UserId);
            try
            {
                if(string.IsNullOrWhiteSpace(request.PlaylistId)) throw new Exception("El id de la lista de reproducción no puede estar vacío");
                if(string.IsNullOrWhiteSpace(request.UserId)) throw new Exception("El id del usuario no puede estar vacío");
                if(!int.TryParse(request.PlaylistId, out int playlistId)) throw new Exception("El id de la lista de reproducción debe ser un número entero positivo");
                if(!int.TryParse(request.UserId, out int userId)) throw new Exception("El id del usuario debe ser un número entero positivo");
                var deleteDTO  = new DeletePlaylistDTO
                {
                    PlaylistId = request.PlaylistId
                };
                var message = await _playlistService.DeletePlaylist(playlistId, userId);
                await _monitoringEventService.PublishActionEventAsync(new ActionEvent
                {
                    ActionMessage = $"El usuario {userId} ha eliminado la lista de reproduccion {deleteDTO.PlaylistId}",
                    UserId = userId.ToString(),
                    UserEmail = request.UserEmail,
                    UrlMethod = $"DELETE/listas-reproduccion/{playlistId}",
                    Service = "PlaylistService"
                });
                return new DeletePlaylistResponse { Response = message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al eliminar la lista de reproducción {PlaylistId} para el usuario {UserId}", request.PlaylistId, request.UserId);
                await _monitoringEventService.PublishErrorEventAsync(new ErrorEvent
                {
                    ErrorMessage = $"Error al eliminar la lista de reproduccion {request.PlaylistId} del usuario {request.UserId}: {ex.Message}",
                    UserId = request.UserId,
                    UserEmail = request.UserEmail,
                    Service = "PlaylistService"
                });
                throw new RpcException(new Status(StatusCode.Internal, $"Error al eliminar la lista de reproducción: {ex.Message}"));
            }
        }
    }
}