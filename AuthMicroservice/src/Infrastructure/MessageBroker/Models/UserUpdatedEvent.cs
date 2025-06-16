using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthMicroservice.src.Infrastructure.MessageBroker.Models
{
    public class UserUpdatedEvent
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UpdatedAt { get; set; } = string.Empty;
    }
}