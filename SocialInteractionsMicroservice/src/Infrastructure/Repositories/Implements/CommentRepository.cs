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
    public class CommentRepository : ICommentRepository
    {
        private readonly SocialInteractionsContext _context;

        public CommentRepository(SocialInteractionsContext context)
        {
            _context = context;
        }

        public async Task<List<Comment>> GetVideoComments(string videoId)
        {
            return await _context.Comments
                .AsNoTracking()
                .Where(c => c.VideoId == videoId)
                .ToListAsync();
        }

        public async Task MakeComment(string videoId, string commentText)
        {
            await _context.Comments.AddAsync(new Comment
            {
                VideoId = videoId,
                Content = commentText
            });

            await _context.SaveChangesAsync();
        }
    }
}