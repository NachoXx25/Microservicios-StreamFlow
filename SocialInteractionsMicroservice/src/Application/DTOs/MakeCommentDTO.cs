using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialInteractionsMicroservice.src.Application.DTOs
{
    public class MakeCommentDTO
    {
        public required string VideoId { get; set; }
        public required string Comment { get; set; }
    }
}