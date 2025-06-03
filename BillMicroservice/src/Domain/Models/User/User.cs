using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace BillMicroservice.src.Domain.Models.User
{
    public class User : IdentityUser<int>
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required bool Status { get; set; }
        public required int RoleId { get; set; }
        public Role Role { get; set; } = null!;
    }
}