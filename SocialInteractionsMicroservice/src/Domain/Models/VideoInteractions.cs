using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.EntityFrameworkCore;

namespace SocialInteractionsMicroservice.src.Domain.Models
{
    [Collection("video_interactions")]
    public class VideoInteractions
    {
        public ObjectId Id { get; set; }

        public required ObjectId VideoId { get; set; }

        public required int Likes { get; set; } = 0;

        public required List<string> Comments { get; set; } = new List<string>();
    }
}