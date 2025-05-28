using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace SocialInteractionsMicroservice.src.Application.DTOs
{
    public class GetVideoInteractionsDTO
    {
        public required string VideoId { get; set; }

        public int Likes { get; set; }

        public List<string> Comments { get; set; } = new List<string>();
    }
}