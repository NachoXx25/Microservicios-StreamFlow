using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiGateway.src.Application.DTOs.Video
{
    public class GetAllVideosDTO
    {
        public string? Title { get; set; }
        public string? Genre { get; set; }
    }
}