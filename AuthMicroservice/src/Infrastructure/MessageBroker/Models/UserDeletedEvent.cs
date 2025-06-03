using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthMicroservice.src.Infrastructure.MessageBroker.Models
{
    public class UserDeletedEvent
    {
        public int Id { get; set; }
        public bool Status { get; set; }
        public string UpdatedAt { get; set; } = string.Empty;
    }
}