using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BillMicroservice.src.Infrastructure.MessageBroker.Models;

namespace BillMicroservice.src.Infrastructure.Repositories.Interfaces
{
    public interface IUserEventHandlerRepository
    {
        Task HandleUserCreated(UserCreatedEvent userCreatedEvent);

        Task HandleUserUpdated(UserUpdatedEvent userUpdatedEvent);

        Task HandleUserDeleted(UserDeletedEvent userDeletedEvent);
    }
}