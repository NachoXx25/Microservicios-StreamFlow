using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillMicroservice.src.Application.DTOs
{
    public class CreatedBillDTO
    {
        public required int Id { get; set; }

        public required int AmountToPay { get; set; }

        public DateTime? PaymentDate { get; set; } = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time"));
        
        public required string Status { get; set; }

        public required int UserId { get; set; }

        public required DateTime CreatedAt { get; set; } = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time"));
    }
}