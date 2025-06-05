using AuthMicroservice.src.Domain.Models;
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
        /// Agrega un token a la lista negra.
        /// </summary>
        /// <param name="jti">Identificador único del token.</param>
        /// <returns>True si el token fue agregado, de lo contrario false.</returns>
        public async Task<bool> AddTokenToBlacklist(string jti)
        {
            var token = await _context.TokenBlacklists.FirstOrDefaultAsync(x => x.Jti == jti);
            if (token != null) return false; 
            var tokenBlacklist = new TokenBlacklist
            {
                Jti = jti,
                RevokedAt = DateTime.UtcNow
            };
            await _context.TokenBlacklists.AddAsync(tokenBlacklist);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Verifica si un token está en la lista negra.
        /// </summary>
        /// <param name="jti">Token a verificar.</param>
        /// <returns>True si el token está en la lista negra, de lo contrario false.</returns>
        public async Task<bool> IsTokenBlacklistedAsync(string jti)
        {
            return await _context.TokenBlacklists.AnyAsync(x => x.Jti == jti);
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