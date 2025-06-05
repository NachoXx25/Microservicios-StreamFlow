using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillMicroservice.src.Infrastructure.MessageBroker.Models
{
    public class ErrorEvent
    {
        public string ErrorMessage { get; set; } = string.Empty;
        public string Service { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}