using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.EntityFrameworkCore;

namespace MonitoringMicroservice.src.Domain.Models
{
    [Collection("Errors")]
    public class Error
    {
        public ObjectId Id { get; set; }

        public required string Message { get; set; }

        public required string UserId { get; set; }

        public required string UserEmail { get; set; }

        public required DateTime Timestamp { get; set; } = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time"));

    }
}