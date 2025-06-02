using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VideoMicroservice.src.Infrastructure.MessageBroker.Models
{
    public class LikeEvent
    {
        public required string VideoId { get; set; }

        public required string LikeId { get; set; }
    }
}