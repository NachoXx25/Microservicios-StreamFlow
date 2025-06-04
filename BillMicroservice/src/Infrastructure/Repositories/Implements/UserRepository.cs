using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BillMicroservice.src.Application.DTOs;
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

        public async Task<GetUserDTO?> GetUserById(string id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return null;
            }

            return new GetUserDTO
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Status = user.Status,
                RoleId = user.RoleId
            };
        }

        public async Task<bool> UserExists(string id)
        {
            return await _context.Users.FindAsync(id) != null;
        }
    }
}