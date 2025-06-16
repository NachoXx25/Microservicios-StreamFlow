using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SocialInteractionsMicroservice.src.Domain.Models;

namespace SocialInteractionsMicroservice.src.Infrastructure.Repositories.Interfaces
{
    public interface ILikeRepository
    {
        Task GiveLike(string videoId);

        Task<List<Like>> GetVideoLikes(string videoId);
    }
}