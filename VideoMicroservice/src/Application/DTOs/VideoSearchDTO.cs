using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VideoMicroservice.src.Application.DTOs
{
    public class VideoSearchDTO
    {
        public string? Title { get; set; }

        public string? Genre { get; set; }
    }
}