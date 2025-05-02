using AuthMicroservice.src.Infrastructure.Data;
using AuthMicroservice.src.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthMicroservice.src.Infrastructure.Repositories.Implements
{
    public class TokenRepository : ITokenRepository
    {
        private readonly DataContext _context;
        public TokenRepository(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Verifica si un token existe en la base de datos.
        /// </summary>
        /// <param name="jti">Identificador único del token.</param>
        /// <returns>True si el token existe, de lo contrario false.</returns>
        public async Task<bool> VerifyIfTokenExists(string jti)
        {
            return await _context.TokenBlacklists.AnyAsync(x => x.Jti == jti);
        }
    }
}