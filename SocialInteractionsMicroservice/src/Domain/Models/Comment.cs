using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.EntityFrameworkCore;

namespace SocialInteractionsMicroservice.src.Domain.Models
{
    [Collection("Comments")]
    public class Comment
    {
        public ObjectId Id { get; set; }
        public required string VideoId { get; set; }

        public required string Content { get; set; }
    }
}