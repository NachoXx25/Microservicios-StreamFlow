using System.IdentityModel.Tokens.Jwt;
using AuthMicroservice.src.Application.DTOs;
using AuthMicroservice.src.Application.Services.Interfaces;
using AuthMicroservice.src.Domain.Models;
using AuthMicroservice.src.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Serilog;

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
        /// Actualiza la contraseña del usuario autenticado.
        /// </summary>
        /// <param name="updatePasswordDTO">Nueva contraseña.</param>
        /// <returns>Mensaje de éxito o error.</returns>
        public async Task<string> ChangePassword(UpdatePasswordDTO updatePasswordDTO)
        {
            var user = await _userManager.FindByIdAsync(updatePasswordDTO.UserRequestId.ToString()) ?? throw new Exception("No encontrado: Usuario no encontrado");
            var userRole = await _roleManager.FindByIdAsync(user.RoleId.ToString()) ?? throw new Exception("No encontrado: Rol no encontrado");
            if(userRole.Name != "Administrador" && user.Id.ToString() != updatePasswordDTO.UserId) throw new Exception("No tiene permisos para cambiar la contraseña de este usuario");
            if(updatePasswordDTO.CurrentPassword == updatePasswordDTO.NewPassword) throw new Exception("La nueva contraseña no puede ser igual a la actual");
            var token = await _tokenRepository.VerifyIfTokenExists(updatePasswordDTO.Jti);
            if(token) throw new Exception("Token de autenticación no válido.");
            var result = await _userManager.ChangePasswordAsync(user, updatePasswordDTO.CurrentPassword, updatePasswordDTO.NewPassword);
            if (!result.Succeeded) throw new Exception("Contraseña actual incorrecta o no válida.");
            return "Contraseña actualizada correctamente";
        }

        /// <summary>
        /// Valida un token JWT y verifica si está en la lista negra.
        /// </summary>
        /// <param name="token">Token a validar.</param>
        /// <returns>Respuesta de validación del token.</returns>
        public async Task<bool> IsTokenBlacklistedAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jsonToken = tokenHandler.ReadJwtToken(token);
                var jti = jsonToken.Claims?.FirstOrDefault(c => c.Type == "Jti")?.Value ?? 
                  jsonToken.Claims?.FirstOrDefault(c => c.Type == "jti")?.Value;
                Log.Warning("Validando token: {Token}", jti);
                if (string.IsNullOrEmpty(jti))
                {
                    return false; 
                }
                
                return await _tokenRepository.IsTokenBlacklistedAsync(jti);
            }
            catch (Exception)
            {
                return false; 
            }
        }

        /// <summary>
        /// Genera un token JWT para el usuario autenticado.
        /// </summary>
        /// <param name="loginDTO">Credenciales.</param>
        /// <returns>Token JWT generado.</returns>
        public async Task<ReturnUserWithTokenDTO> Login(LoginDTO loginDTO)
        {
            User user = await _userManager.FindByEmailAsync(loginDTO.Email) ?? throw new Exception("Usuario o contraseña incorrectos");
            var result = await _userManager.CheckPasswordAsync(user, loginDTO.Password);
            var role = await _roleManager.FindByIdAsync(user.RoleId.ToString()) ?? throw new Exception("No encontrado: Rol no encontrado");
            if (!result) throw new Exception("Usuario o contraseña incorrectos");
            if(!user.Status) throw new Exception("Usuario inactivo, no puede iniciar sesión");
            var token = await _tokenService.CreateToken(user);
            return new ReturnUserWithTokenDTO
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Token = token,
                RoleName = role.Name ?? throw new ArgumentNullException("No encontrado: Rol no encontrado"),
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
            if(!result) throw new Exception("No encontrado: Token no encontrado o ya eliminado");
            return "Token eliminado correctamente";
        }
    }
}