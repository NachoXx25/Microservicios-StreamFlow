using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.EntityFrameworkCore;

namespace SocialInteractionsMicroservice.src.Domain.Models
{
    [Collection("videos")]
    public class Video
    {
        public ObjectId Id { get; set; }

        public required string Title { get; set; }
        
        public required string Description { get; set; }
        
        public required string Genre { get; set; }

        public required bool IsDeleted { get; set; }  = false;
    }
}