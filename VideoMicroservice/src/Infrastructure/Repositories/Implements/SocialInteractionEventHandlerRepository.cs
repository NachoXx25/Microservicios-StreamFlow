using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VideoMicroservice.src.Infrastructure.Data;
using VideoMicroservice.src.Infrastructure.MessageBroker.Models;
using VideoMicroservice.src.Infrastructure.Repositories.Interfaces;

namespace VideoMicroservice.src.Infrastructure.Repositories.Implements
{
    public class SocialInteractionEventHandlerRepository : ISocialInteractionEventHandlerRepository
    {
        private readonly VideoContext _context;

        public SocialInteractionEventHandlerRepository(VideoContext context)
        {
            _context = context;
        }

        public async Task HandleLikedVideoEvent(LikeEvent likeEvent)
        {
            var bsonId = MongoDB.Bson.ObjectId.Parse(likeEvent.VideoId);

            var video = await _context.Videos.FindAsync(bsonId) ?? throw new Exception("Video no encontrado");

            video.Likes += 1;

            await _context.SaveChangesAsync();
        }
    }
}