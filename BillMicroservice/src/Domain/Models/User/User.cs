using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace BillMicroservice.src.Domain.Models.User
{
    public class User : IdentityUser<int>
    {

        public required int RoleId { get; set; }
        public Role Role { get; set; } = null!;
    }
}