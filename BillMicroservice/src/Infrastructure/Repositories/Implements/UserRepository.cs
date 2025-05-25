using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BillMicroservice.src.Infrastructure.Data;
using BillMicroservice.src.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BillMicroservice.src.Infrastructure.Repositories.Implements
{
    public class UserRepository : IUserRepository
    {
        private readonly BillContext _context;

        public UserRepository(BillContext context)
        {
            _context = context;
        }

        public async Task<bool> UserExists(int id)
        {
            return await _context.Users.FindAsync(id) != null;
        }
    }
}