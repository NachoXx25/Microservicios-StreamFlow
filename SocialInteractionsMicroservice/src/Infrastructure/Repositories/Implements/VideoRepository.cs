using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SocialInteractionsMicroservice.src.Domain.Models;
using SocialInteractionsMicroservice.src.Infrastructure.Data;
using SocialInteractionsMicroservice.src.Infrastructure.Repositories.Interfaces;

namespace SocialInteractionsMicroservice.src.Infrastructure.Repositories.Implements
{
    public class VideoRepository : IVideoRepository
    {
        private readonly SocialInteractionsContext _context;

        public VideoRepository(SocialInteractionsContext context)
        {
            _context = context;
        }

        public async Task<Video?> VideoExists(string videoId)
        {
            return await _context.Videos.AsNoTracking().FirstOrDefaultAsync(v => v.Id.ToString() == videoId);
        }
    }
}