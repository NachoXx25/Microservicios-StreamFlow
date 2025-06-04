using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlaylistMicroservice.src.Application.DTOs
{
    public class PlaylistDTO
    {
        public int Id { get; set; }
        public required string Name { get; set; } 
    }
}