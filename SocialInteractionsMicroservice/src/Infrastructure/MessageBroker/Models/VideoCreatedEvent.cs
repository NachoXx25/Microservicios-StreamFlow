using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialInteractionsMicroservice.src.Infrastructure.MessageBroker.Models
{
    public class VideoCreatedEvent
    {
        public required string Id { get; set; }

        public required string Title { get; set; }

        public required string Description { get; set; }

        public required string Genre { get; set; }

        public required bool IsDeleted { get; set; }
    }
}