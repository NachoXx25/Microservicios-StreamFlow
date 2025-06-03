using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillMicroservice.src.Application.DTOs
{
    public class UpdatedBillDTO
    {
        public required int Id { get; set; }

        public required int UserId { get; set; }

        public required string UserEmail { get; set; }

        public required string FirstName { get; set; }

        public required string LastName { get; set; }

        public required int Amount { get; set; }

        public required string StatusName { get; set; }
    }
}