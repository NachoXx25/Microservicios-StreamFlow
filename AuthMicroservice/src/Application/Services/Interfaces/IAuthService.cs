using AuthMicroservice.src.Application.DTOs;

namespace AuthMicroservice.src.Application.Services.Interfaces
{
    public interface IAuthService
    {
        /// <summary>
        /// Genera un token JWT para el usuario autenticado.
        /// </summary>
        /// <param name="loginDTO">Credenciales.</param>
        /// <returns>Token JWT generado.</returns>
        Task<ReturnUserWithTokenDTO> Login(LoginDTO loginDTO);

        /// <summary>
        /// Cierra sesión y elimina el token JWT del usuario autenticado.
        /// </summary>
        /// <param name="jti">Identificador único del token.</param>
        /// <returns>True si el token fue eliminado, de lo contrario false.</returns>
        Task<string> Logout(string jti);
    }
}