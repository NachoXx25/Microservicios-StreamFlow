using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using SocialInteractionsMicroservice.src.Application.Services.Interfaces;
using SocialInteractionsMicroservice.src.Domain.Models;

namespace SocialInteractionsMicroservice.Services
{
    public class SocialInteractionsGrpcService : Protos.SocialInteractionsGrpcService.SocialInteractionsGrpcServiceBase
    {
        private readonly ISocialInteractionsService _socialInteractionsService;

        public SocialInteractionsGrpcService(ISocialInteractionsService socialInteractionsService)
        {
            _socialInteractionsService = socialInteractionsService;
        }

        public override async Task<Protos.GetVideoLikesAndCommentsResponse> GetVideoLikesAndComments(Protos.GetVideoLikesAndCommentsRequest request, ServerCallContext context)
        {
            try
            {
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
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public override async Task<Protos.GiveLikeResponse> GiveLike(Protos.GiveLikeRequest request, ServerCallContext context)
        {

            try
            {
                var response = await _socialInteractionsService.GiveLike(request.VideoId);
                return new Protos.GiveLikeResponse
                {
                    VideoId = response.VideoId,
                    Likes = response.Likes
                };
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public override async Task<Protos.MakeCommentResponse> MakeComment(Protos.MakeCommentRequest request, ServerCallContext context)
        {

            try
            {
                var response = await _socialInteractionsService.MakeComment(request.VideoId, request.Comment);
                return new Protos.MakeCommentResponse
                {
                    VideoId = response.VideoId,
                    Comment = response.Comment
                };
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }
    }
}