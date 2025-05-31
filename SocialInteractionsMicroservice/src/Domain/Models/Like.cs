using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.EntityFrameworkCore;

namespace SocialInteractionsMicroservice.src.Domain.Models
{
    [Collection("Likes")]
    public class Like
    {
        public ObjectId Id { get; set; }
        public required string VideoId { get; set; }
    }
}