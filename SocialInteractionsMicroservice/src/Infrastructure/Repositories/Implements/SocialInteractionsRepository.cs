using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using SocialInteractionsMicroservice.src.Application.DTOs;
using SocialInteractionsMicroservice.src.Domain.Models;
using SocialInteractionsMicroservice.src.Infrastructure.Data;
using SocialInteractionsMicroservice.src.Infrastructure.Repositories.Interfaces;

namespace SocialInteractionsMicroservice.src.Infrastructure.Repositories.Implements
{
    public class SocialInteractionsRepository : ISocialInteractionsRepository
    {
        private readonly SocialInteractionsContext _context;

        public SocialInteractionsRepository(SocialInteractionsContext context)
        {
            _context = context;
        }

        public async Task<GetVideoInteractionsDTO> GetVideoInteractions(ObjectId videoId)
        {
            var videoInteractions = await _context.VideoInteractions.AsNoTracking().FirstOrDefaultAsync(v => v.VideoId == videoId);

            //Si no existe la interacción, se crea una nueva
            if (videoInteractions == null)
            {
                videoInteractions = new VideoInteractions
                {
                    VideoId = videoId,
                    Likes = 0,
                    Comments = new List<string>()
                };

                await _context.VideoInteractions.AddAsync(videoInteractions);
                await _context.SaveChangesAsync();
            }

            var videoInteractionsDTO = new GetVideoInteractionsDTO
            {
                VideoId = videoInteractions.Id.ToString(),
                Likes = videoInteractions.Likes,
                Comments = videoInteractions.Comments
            };

            return videoInteractionsDTO;
        }

        public async Task<GiveLikeDTO> GiveLike(ObjectId videoId)
        {
            var videoInteractions = await _context.VideoInteractions.FirstOrDefaultAsync(v => v.VideoId == videoId);

            if (videoInteractions == null)
            {
                videoInteractions = new VideoInteractions
                {
                    VideoId = videoId,
                    Likes = 0,
                    Comments = new List<string>()
                };

                await _context.VideoInteractions.AddAsync(videoInteractions);
                await _context.SaveChangesAsync();
            }
  
            videoInteractions.Likes++;
    
            await _context.SaveChangesAsync();

            var giveLikeDTO = new GiveLikeDTO
            {
                VideoId = videoId.ToString(),
                Likes = videoInteractions.Likes
            };

            return giveLikeDTO;
        }

        public async Task<MakeCommentDTO> MakeComment(ObjectId videoId, string comment)
        {
            var videoInteractions = await _context.VideoInteractions.FirstOrDefaultAsync(v => v.VideoId == videoId);

            if (videoInteractions == null)
            {
                videoInteractions = new VideoInteractions
                {
                    VideoId = videoId,
                    Likes = 0,
                    Comments = new List<string>()
                };

                await _context.VideoInteractions.AddAsync(videoInteractions);
                await _context.SaveChangesAsync();
            }

            videoInteractions.Comments.Add(comment);

            await _context.SaveChangesAsync();

            var makeCommentDTO = new MakeCommentDTO
            {
                VideoId = videoId.ToString(),    
                Comment = comment
            };

            return makeCommentDTO;
        }
    }
}