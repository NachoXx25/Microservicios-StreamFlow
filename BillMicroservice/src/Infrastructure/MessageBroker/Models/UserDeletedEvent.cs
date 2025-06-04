using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillMicroservice.src.Infrastructure.MessageBroker.Models
{
    public class UserDeletedEvent
    {
        public required int Id { get; set; }

        public required bool Status { get; set; }

        public required DateTime UpdatedAt { get; set; }
    }
}