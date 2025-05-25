using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillMicroservice.src.Domain.Models.Bill
{
    public class Status
    {
        public int Id { get; set; }
        public required string Name { get; set; } = null!;
    }
}