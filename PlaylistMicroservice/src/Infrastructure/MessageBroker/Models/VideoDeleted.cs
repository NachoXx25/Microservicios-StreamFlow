using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlaylistMicroservice.src.Infrastructure.MessageBroker.Models
{
    public class VideoDeleted
    {
        public string Id { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } 
    }
}