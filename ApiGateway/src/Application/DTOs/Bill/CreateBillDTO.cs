using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiGateway.src.Application.DTOs.Bill
{
    public class CreateBillDTO
    {
        public required string UserId { get; set; }

        public required int Amount { get; set; }

        public required string BillStatus { get; set; }
    }
}