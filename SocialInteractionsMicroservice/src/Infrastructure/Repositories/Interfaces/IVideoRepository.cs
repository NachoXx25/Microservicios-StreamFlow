using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SocialInteractionsMicroservice.src.Domain.Models;

namespace SocialInteractionsMicroservice.src.Infrastructure.Repositories.Interfaces
{
    public interface IVideoRepository
    {
        Task<Video?> VideoExists(string videoId);
    }
}