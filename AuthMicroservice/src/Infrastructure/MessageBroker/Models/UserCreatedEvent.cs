using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthMicroservice.src.Infrastructure.MessageBroker.Models
{
    public class UserCreatedEvent
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public bool Status { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
    }
}