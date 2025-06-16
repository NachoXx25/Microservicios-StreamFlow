using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmailMicroservice.src.Infrastructure.MessageBroker.Models;

namespace EmailMicroservice.src.Interfaces
{
    public interface IBillEventHandler
    {
        public Task HandleBillUpdatedEvent(BillUpdated billEvent);
    }
}