using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialInteractionsMicroservice.src.Application.DTOs
{
    public class GiveLikeDTO
    {
        public required string VideoId { get; set; }
        public int Likes { get; set; }
    }
}