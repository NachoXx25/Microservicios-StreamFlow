using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlaylistMicroservice.src.Infrastructure.MessageBroker.Models
{
    public class VideoCreated
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }
}