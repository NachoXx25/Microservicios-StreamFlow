using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmailMicroservice.src.Infrastructure.MessageBroker.Models
{
    public class BillUpdated
    {
        public string BillId { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string BillStatus { get; set; } = string.Empty;
        public string BillAmount { get; set; } = string.Empty;  
    }
}