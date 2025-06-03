using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillMicroservice.src.Infrastructure.MessageBroker.Models
{
    public class UserUpdatedEvent
    {
        public required int Id { get; set; }

        public required string FirstName { get; set; }

        public required string LastName { get; set; }

        public required string Email { get; set; }

        public required DateTime UpdatedAt { get; set; }
    }
}