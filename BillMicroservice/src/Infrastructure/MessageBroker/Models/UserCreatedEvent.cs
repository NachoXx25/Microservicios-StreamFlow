using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillMicroservice.src.Infrastructure.MessageBroker.Models
{
    public class UserCreatedEvent
    {
        public required int Id { get; set; }

        public required string FirstName { get; set; }

        public required string LastName { get; set; }

        public required string Email { get; set; }

        public required int RoleId { get; set; }

        public required bool Status { get; set; }

        public required DateTime CreatedAt { get; set; }
    }
}