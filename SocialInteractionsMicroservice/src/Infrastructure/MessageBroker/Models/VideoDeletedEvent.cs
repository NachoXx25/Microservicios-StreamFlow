using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialInteractionsMicroservice.src.Infrastructure.MessageBroker.Models
{
    public class VideoDeletedEvent
    {
        public required string Id { get; set; }

        public required bool IsDeleted { get; set; }
    }
}