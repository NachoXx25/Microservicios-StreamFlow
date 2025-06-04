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
    public class LikeRepository : ILikeRepository
    {
        private readonly SocialInteractionsContext _context;

        public LikeRepository(SocialInteractionsContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<Like>> GetVideoLikes(string videoId)
        {
            return await _context.Likes.AsNoTracking().Where(l => l.VideoId == videoId).ToListAsync();
        }

        public async Task GiveLike(string videoId)
        {
            await _context.Likes.AddAsync(new Like
            {
                VideoId = videoId,
            });

            await _context.SaveChangesAsync();
        }
    }
}