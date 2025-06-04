using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SocialInteractionsMicroservice.src.Application.DTOs;

namespace SocialInteractionsMicroservice.src.Application.Services.Interfaces
{
    public interface ISocialInteractionsService
    {
        Task<GiveLikeDTO> GiveLike(string videoId);

        Task<MakeCommentDTO> MakeComment(string videoId, string comment);

        Task<GetVideoInteractionsDTO> GetVideoInteractions(string videoId);
    }
}