using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserMicroservice.src.Domain;
using UserMicroservice.src.Infrastructure.Data;
using UserMicroservice.src.Infrastructure.Repositories.Interfaces;

namespace UserMicroservice.src.Infrastructure.Repositories.Implements
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        public UserRepository(DataContext context)
        {
            _context = context;
        }
/// <summary>
        /// Hace un borrado logico del usuario
        /// </summary>
        /// <param name="user">Usuario a borrar.</param>
        public async Task DeleteUser(User user)
        {
            try
            {
                user.Status = false;
                await _context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                throw new Exception("Error al procesar la operación", ex);
            }
        }
    }

}