using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VideoMicroservice.src.Application.DTOs
{
    public class GetVideoDTO
    {
        public required string Id { get; set; }

        public required string Title { get; set; }

        public required string Description { get; set; }

        public required string Genre { get; set; }

        public required int Likes { get; set; }
    }
}