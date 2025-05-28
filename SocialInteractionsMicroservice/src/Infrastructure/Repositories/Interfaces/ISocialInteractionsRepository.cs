using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using SocialInteractionsMicroservice.src.Application.DTOs;

namespace SocialInteractionsMicroservice.src.Infrastructure.Repositories.Interfaces
{
    public interface ISocialInteractionsRepository
    {
        Task<GiveLikeDTO> GiveLike(ObjectId videoId);

        Task<MakeCommentDTO> MakeComment(ObjectId videoId, string comment);

        Task<GetVideoInteractionsDTO> GetVideoInteractions(ObjectId videoId);
    }
}