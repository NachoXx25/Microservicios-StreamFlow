using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SocialInteractionsMicroservice.src.Infrastructure.MessageBroker.Models;

namespace SocialInteractionsMicroservice.src.Infrastructure.Repositories.Interfaces
{
    public interface IVideoEventHandlerRepository
    {
        Task HandleVideoCreatedEventAsync(VideoCreatedEvent videoCreatedEvent);

        Task HandleVideoUpdatedEventAsync(VideoUpdatedEvent videoUpdatedEvent);

        Task HandleVideoDeletedEventAsync(VideoDeletedEvent videoDeletedEvent);
    }
}