using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlaylistMicroservice.src.Infrastructure.MessageBroker.Models;

namespace PlaylistMicroservice.src.Infrastructure.Repositories.Interfaces
{
    public interface IVideoEventHandlerRepository
    {
        public Task HandleVideoCreatedEvent(VideoCreated video);

        public Task HandleVideoUpdatedEvent(VideoUpdated video);

        public Task HandleVideoDeletedEvent(VideoDeleted video);
    }
}