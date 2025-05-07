using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthMicroservice.src.Application.Services.Interfaces;
using AuthMicroservice.src.Domain.Models;
using AuthMicroservice.src.Infrastructure.Repositories.Interfaces;
using DotNetEnv;
using Microsoft.IdentityModel.Tokens;

namespace AuthMicroservice.src.Application.Services.Implements
{
    public class TokenService : ITokenService
    {
        private readonly ITokenRepository _tokenRepository;
        public TokenService(ITokenRepository tokenRepository)
        {
            _tokenRepository = tokenRepository;
        }
        /// <summary>
        /// Crea un token JWT para el usuario proporcionado.
        /// </summary>
        /// <param name="user">Usuario para el cual se generará el token.</param>
        /// <returns>Token generado</returns>
        public async Task<string> CreateToken(User user)
        {
            var Jti = Guid.NewGuid().ToString();
            bool tokenExists = true;
            while (tokenExists)
            {
                tokenExists = await _tokenRepository.VerifyIfTokenExists(Jti);
            }
            
            var claims = new List<Claim>
            {
                new Claim("Jti", Jti),
                new Claim("Id", user.Id.ToString()),
                new Claim("Email", user.Email?.ToString() ?? throw new ArgumentNullException("Email no encontrado")),
                new Claim("Role", user.Role?.Name?.ToString() ?? throw new ArgumentNullException("Rol no encontrado")),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Env.GetString("JWT_SECRET") ?? throw new ArgumentNullException("JWT_SECRET no encontrado")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMonths(2),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}