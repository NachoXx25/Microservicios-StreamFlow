using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonitoringMicroservice.src.Infrastructure.MessageBroker.Models
{
    public class ActionEvent
    {
  
        public required string Name { get; set; }

        public required string UserId { get; set; }

        public required string UserEmail { get; set; }

        public required string MethodUrl { get; set; }

        public required DateTime Timestamp { get; set; } = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time"));
    }
}