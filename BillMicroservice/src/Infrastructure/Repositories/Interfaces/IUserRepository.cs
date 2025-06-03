using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BillMicroservice.src.Application.DTOs;
using BillMicroservice.src.Domain.Models.User;

namespace BillMicroservice.src.Infrastructure.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> UserExists(int id);

        Task<GetUserDTO?> GetUserById(int id);
    }
}