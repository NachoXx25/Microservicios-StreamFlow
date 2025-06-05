using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiGateway.src.Application.DTOs.Video
{
    public class UploadVideoDTO
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string Genre { get; set; }
    }
}