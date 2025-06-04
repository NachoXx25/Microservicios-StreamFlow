using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonitoringMicroservice.src.Infrastructure.MessageBroker.Models
{
    public class ErrorEvent
    {
        public required string ErrorMessage { get; set; }

        public required string Service { get; set; }

        public required string UserId { get; set; }

        public required string UserEmail { get; set; }

        public required DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}