using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonitoringMicroservice.src.Application.DTOs
{
    public class ActionDTO
    {
        public required string Id { get; set; }

        public required string Name { get; set; }

        public required string UserId { get; set; }

        public required string UserEmail { get; set; }

        public required string MethodUrl { get; set; }

        public required DateTime Timestamp { get; set; } = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time"));
    }
}