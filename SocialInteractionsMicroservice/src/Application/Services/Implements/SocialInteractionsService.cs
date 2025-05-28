using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SocialInteractionsMicroservice.src.Application.DTOs;
using SocialInteractionsMicroservice.src.Application.Services.Interfaces;
using SocialInteractionsMicroservice.src.Infrastructure.Repositories.Interfaces;

namespace SocialInteractionsMicroservice.src.Application.Services.Implements
{
    public class SocialInteractionsService : ISocialInteractionsService
    {
        private readonly ISocialInteractionsRepository _socialInteractionsRepository;
        private readonly IVideoRepository _videoRepository;

        public SocialInteractionsService(ISocialInteractionsRepository socialInteractionsRepository, IVideoRepository videoRepository)
        {
            _socialInteractionsRepository = socialInteractionsRepository;
            _videoRepository = videoRepository;
        }

        public async Task<GetVideoInteractionsDTO> GetVideoInteractions(string videoId)
        {
            var video = await _videoRepository.VideoExists(videoId) ?? throw new Exception("Video no encontrado");

            return await _socialInteractionsRepository.GetVideoInteractions(video.Id);
        }

        public async Task<GiveLikeDTO> GiveLike(string videoId)
        {
            var video = await _videoRepository.VideoExists(videoId) ?? throw new Exception("Video no encontrado");

            if (video.IsDeleted)
            {
                throw new Exception("El video ha sido eliminado, no se pueden dar likes");
            }
            
            return await _socialInteractionsRepository.GiveLike(video.Id);
        }

        public async Task<MakeCommentDTO> MakeComment(string videoId, string comment)
        {
            var video = await _videoRepository.VideoExists(videoId) ?? throw new Exception("Video no encontrado");

            if (video.IsDeleted)
            {
                throw new Exception("El video ha sido eliminado, no se puede comentar");
            }

            return await _socialInteractionsRepository.MakeComment(video.Id, comment);
        }
    }
}