using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VideoMicroservice.src.Infrastructure.MessageBroker.Models;

namespace VideoMicroservice.src.Infrastructure.Repositories.Interfaces
{
    public interface ISocialInteractionEventHandlerRepository
    {
        Task HandleLikedVideoEvent(LikeEvent likeEvent);
    }
}