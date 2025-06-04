using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonitoringMicroservice.src.Application.DTOs
{
    public class ErrorDTO
    {
        public required string Id { get; set; }

        public required string Message { get; set; }

        public required string UserId { get; set; }

        public required string UserEmail { get; set; }

        public required DateTime Timestamp { get; set; } = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time"));
    }
}