using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Serilog;
using SocialInteractionsMicroservice.Protos;
using SocialInteractionsMicroservice.src.Application.Services.Interfaces;
using SocialInteractionsMicroservice.src.Domain.Models;
using SocialInteractionsMicroservice.src.Infrastructure.MessageBroker.Models;

namespace SocialInteractionsMicroservice.Services
{
    public class SocialInteractionsGrpcService : Protos.SocialInteractionsGrpcService.SocialInteractionsGrpcServiceBase
    {
        private readonly ISocialInteractionsService _socialInteractionsService;

        private readonly IMonitoringEventService _monitoringEventService;

        public SocialInteractionsGrpcService(ISocialInteractionsService socialInteractionsService, IMonitoringEventService monitoringEventService)
        {
            _socialInteractionsService = socialInteractionsService;
            _monitoringEventService = monitoringEventService;
        }

        public override async Task<Protos.GetVideoLikesAndCommentsResponse> GetVideoLikesAndComments(Protos.GetVideoLikesAndCommentsRequest request, ServerCallContext context)
        {
            try
            {
                await _monitoringEventService.PublishActionEventAsync(new ActionEvent
                {
                    ActionMessage = "Obtener likes y comentarios de un video",
                    Service = "SocialInteractionsMicroservice",
                    UserId = request.UserData.Id,
                    UserEmail = request.UserData.Email,
                    UrlMethod = $"GET/interacciones/{request.VideoId}"
                });

                if (string.IsNullOrWhiteSpace(request.UserData.Id))
                {
                    throw new Exception("No autenticado: se require un usuario autenticado para obtener likes y comentarios de un video.");
                }

                var response = await _socialInteractionsService.GetVideoInteractions(request.VideoId);

                var likes = response.Likes.Select(l => new Protos.Like
                {
                    LikeId = l.Id.ToString(),
                }).ToList();

                var comments = response.Comments.Select(c => new Protos.Comment
                {
                    CommentId = c.Id.ToString(),
                    Content = c.Content,
                }).ToList();

                return new Protos.GetVideoLikesAndCommentsResponse
                {
                    VideoId = response.VideoId,
                    Likes = { likes },
                    Comments = { comments }
                };
            }
            catch (Exception ex)
            {
                await _monitoringEventService.PublishErrorEventAsync(new ErrorEvent
                {
                    ErrorMessage = $"Error al obtener likes y comentarios de un video:{ex.Message}",
                    Service = "SocialInteractionsMicroservice",
                    UserId = request.UserData.Id,
                    UserEmail = request.UserData.Email,
                });
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public override async Task<Protos.GiveLikeResponse> GiveLike(Protos.GiveLikeRequest request, ServerCallContext context)
        {
            try
            {
                await _monitoringEventService.PublishActionEventAsync(new ActionEvent
                {
                    ActionMessage = "Dar like",
                    Service = "SocialInteractionsMicroservice",
                    UserId = request.UserData.Id,
                    UserEmail = request.UserData.Email,
                    UrlMethod = $"POST/interacciones/{request.VideoId}/likes"
                });

                if (string.IsNullOrWhiteSpace(request.UserData.Id))
                {
                    throw new Exception("No autenticado: se requiere un usuario autenticado para dar like a un video.");
                }

                var response = await _socialInteractionsService.GiveLike(request.VideoId);
                return new Protos.GiveLikeResponse
                {
                    VideoId = response.VideoId,
                    Likes = response.Likes
                };
            }
            catch (Exception ex)
            {
                await _monitoringEventService.PublishErrorEventAsync(new ErrorEvent
                {
                    ErrorMessage = $"Error al dar like al video: {ex.Message}",
                    Service = "SocialInteractionsMicroservice",
                    UserId = request.UserData.Id,
                    UserEmail = request.UserData.Email,
                });
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public override async Task<Protos.MakeCommentResponse> MakeComment(Protos.MakeCommentRequest request, ServerCallContext context)
        {
            try
            {
                await _monitoringEventService.PublishActionEventAsync(new ActionEvent
                {
                    ActionMessage = "Dejar comentario",
                    Service = "SocialInteractionsMicroservice",
                    UserId = request.UserData.Id,
                    UserEmail = request.UserData.Email,
                    UrlMethod = $"POST/interacciones/{request.VideoId}/comentarios"
                });

                if (string.IsNullOrWhiteSpace(request.UserData.Id))
                {
                    throw new Exception("No autenticado: se requiere un usuario autenticado para dejar un comentario en un video.");
                }

                var response = await _socialInteractionsService.MakeComment(request.VideoId, request.Comment);
                return new Protos.MakeCommentResponse
                {
                    VideoId = response.VideoId,
                    Comment = response.Comment
                };
            }
            catch (Exception ex)
            {
                await _monitoringEventService.PublishErrorEventAsync(new ErrorEvent
                {
                    ErrorMessage = $"Error al dejar comentario en el video: {ex.Message}",
                    Service = "SocialInteractionsMicroservice",
                    UserId = request.UserData.Id,
                    UserEmail = request.UserData.Email,
                });
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public override Task<CheckHealthResponse> CheckHealth(Empty request, ServerCallContext context)
        {
            try
            {
                return Task.FromResult(new CheckHealthResponse
                {
                    IsRunning = true
                });
            }
            catch (Exception ex)
            {
                Log.Error($"Error al verificar la salud del servicio: {ex.Message}");
                throw new RpcException(new Status(StatusCode.Internal, "Error al verificar la salud del servicio de interacciones sociales."));
            }
        }
    }
}