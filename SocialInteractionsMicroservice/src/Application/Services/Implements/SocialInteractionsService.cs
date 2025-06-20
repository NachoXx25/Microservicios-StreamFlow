using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using SocialInteractionsMicroservice.Services;
using SocialInteractionsMicroservice.src.Application.DTOs;
using SocialInteractionsMicroservice.src.Application.Services.Interfaces;
using SocialInteractionsMicroservice.src.Infrastructure.Repositories.Interfaces;

namespace SocialInteractionsMicroservice.src.Application.Services.Implements
{
    public class SocialInteractionsService : ISocialInteractionsService
    {
        private readonly ILikeRepository _likeRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IVideoRepository _videoRepository;

        private readonly ISocialInteractionsEventService _socialInteractionsEventService;

        public SocialInteractionsService(ILikeRepository likeRepository, ICommentRepository commentRepository, IVideoRepository videoRepository, ISocialInteractionsEventService socialInteractionsEventService)
        {
            _likeRepository = likeRepository;
            _commentRepository = commentRepository;
            _videoRepository = videoRepository;
            _socialInteractionsEventService = socialInteractionsEventService;
        }

        public async Task<GetVideoInteractionsDTO> GetVideoInteractions(string videoId)
        {
            if (string.IsNullOrWhiteSpace(videoId))
            {
                throw new ArgumentException("El id no puede ser nulo o vacío.");
            }

            var video = await _videoRepository.VideoExists(videoId) ?? throw new KeyNotFoundException("Video no encontrado.");

            var likes = await _likeRepository.GetVideoLikes(videoId);
            var comments = await _commentRepository.GetVideoComments(videoId);

            return new GetVideoInteractionsDTO
            {
                VideoId = videoId,
                Likes = likes,
                Comments = comments
            };
        }

        public async Task<GiveLikeDTO> GiveLike(string videoId)
        {
            if (string.IsNullOrWhiteSpace(videoId))
            {
                throw new ArgumentException("El id no puede ser nulo o vacío.");
            }

            var video = await _videoRepository.VideoExists(videoId) ?? throw new KeyNotFoundException("Video no encontrado.");

            if (video.IsDeleted) { 
                throw new InvalidOperationException("El video ha sido eliminado, no se puede dar like.");
            }

            await _likeRepository.GiveLike(videoId);

            await _socialInteractionsEventService.PublishLikeEvent(videoId);

            var likes = await _likeRepository.GetVideoLikes(videoId);

            return new GiveLikeDTO
            {
                VideoId = videoId,
                Likes = likes.Count
            };
        }

        public async Task<MakeCommentDTO> MakeComment(string videoId, string comment)
        {
            if (string.IsNullOrWhiteSpace(videoId))
            {
                throw new ArgumentException("El id no puede ser nulo o vacío.");
            }

            if (string.IsNullOrWhiteSpace(comment))
            {
                throw new ArgumentException("El comentario no puede ser nulo o vacío.");
            }

            var video = await _videoRepository.VideoExists(videoId) ?? throw new KeyNotFoundException("Video no encontrado.");
           
            if (video.IsDeleted)
            {
                throw new InvalidOperationException("El video ha sido eliminado, no se puede comentar.");
            }

            await _commentRepository.MakeComment(videoId, comment);

            return new MakeCommentDTO
            {
                VideoId = videoId,
                Comment = comment,
            };
        }
    }
}