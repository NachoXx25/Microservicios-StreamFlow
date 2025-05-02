using AuthMicroservice.src.Application.DTOs;
using AuthMicroservice.src.Application.Services.Interfaces;
using AuthMicroservice.src.Domain.Models;
using AuthMicroservice.src.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace AuthMicroservice.src.Application.Services.Implements
{
    public class AuthService : IAuthService
    {   
        private readonly ITokenService _tokenService;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly ITokenRepository _tokenRepository;
        public AuthService(ITokenService tokenService, UserManager<User> userManager, RoleManager<Role> roleManager, ITokenRepository tokenRepository)
        {   
            {
                _tokenService = tokenService;
                _userManager = userManager;
                _roleManager = roleManager;
                _tokenRepository = tokenRepository;
            }
        }

        /// <summary>
        /// Genera un token JWT para el usuario autenticado.
        /// </summary>
        /// <param name="loginDTO">Credenciales.</param>
        /// <returns>Token JWT generado.</returns>
        public async Task<ReturnUserWithTokenDTO> Login(LoginDTO loginDTO)
        {
            User user = await _userManager.FindByEmailAsync(loginDTO.Email) ?? throw new Exception("Usuario no encontrado");
            var result = await _userManager.CheckPasswordAsync(user, loginDTO.Password);
            var role = await _roleManager.FindByIdAsync(user.RoleId.ToString()) ?? throw new Exception("Rol no encontrado");
            if (!result) throw new Exception("Contraseña o usuario incorrectos");
            if(!user.Status) throw new Exception("Usuario inactivo, no puede iniciar sesión");
            var token = await _tokenService.CreateToken(user);
            return new ReturnUserWithTokenDTO
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Token = token,
                RoleName = role.Name ?? throw new ArgumentNullException("Rol no encontrado"),
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                IsActive = user.Status,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }

        /// <summary>
        /// Cierra sesión y elimina el token JWT del usuario autenticado.
        /// </summary>
        /// <param name="jti">Identificador único del token.</param>
        /// <returns>Mensaje de éxito o error.</returns>
        public async Task<string> Logout(string jti)
        {
            var result = await _tokenRepository.AddTokenToBlacklist(jti);
            if(!result) throw new Exception("Token no encontrado o ya eliminado");
            return "Token eliminado correctamente";
        }
    }
}