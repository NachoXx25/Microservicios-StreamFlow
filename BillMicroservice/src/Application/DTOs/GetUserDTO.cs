using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillMicroservice.src.Application.DTOs
{
    public class GetUserDTO
    {
        public required int Id { get; set; }

        public required string FirstName { get; set; }

        public required string LastName { get; set; }

        public required string Email { get; set; }

        public required bool Status { get; set; }

        public required int RoleId { get; set; }
    }
}