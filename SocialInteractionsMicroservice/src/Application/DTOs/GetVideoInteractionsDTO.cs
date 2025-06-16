using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using SocialInteractionsMicroservice.src.Domain.Models;

namespace SocialInteractionsMicroservice.src.Application.DTOs
{
    public class GetVideoInteractionsDTO
    {
        public required string VideoId { get; set; }

        public required List<Like> Likes { get; set; } = new List<Like>();

        public required List<Comment> Comments { get; set; } = new List<Comment>();
    }
}