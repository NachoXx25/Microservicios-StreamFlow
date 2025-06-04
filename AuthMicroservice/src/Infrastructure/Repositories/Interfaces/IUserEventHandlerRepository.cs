using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthMicroservice.src.Infrastructure.MessageBroker.Models;

namespace AuthMicroservice.src.Infrastructure.Repositories.Interfaces
{
    public interface IUserEventHandlerRepository
    {
        Task HandleUserCreatedEvent(UserCreatedEvent userEvent);
        Task HandleUserUpdatedEvent(UserUpdatedEvent userEvent);
        Task HandleUserDeletedEvent(UserDeletedEvent userEvent);
    }
}