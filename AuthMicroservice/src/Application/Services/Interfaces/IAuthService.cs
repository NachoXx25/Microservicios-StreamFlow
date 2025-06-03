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
        /// Actualiza la contraseña del usuario autenticado.
        /// </summary>
        /// <param name="updatePasswordDTO">Nueva contraseña.</param>
        /// <returns>Mensaje de éxito o error.</returns>
        Task<string> ChangePassword(UpdatePasswordDTO updatePasswordDTO);

        /// <summary>
        /// Cierra sesión y elimina el token JWT del usuario autenticado.
        /// </summary>
        /// <param name="jti">Identificador único del token.</param>
        /// <returns>True si el token fue eliminado, de lo contrario false.</returns>
        Task<string> Logout(string jti);

        /// <summary>
        /// Valida un token JWT y verifica si está en la lista negra.
        /// </summary>
        /// <param name="token">Token a validar.</param>
        /// <returns>Respuesta de validación del token.</returns>
        Task<bool> IsTokenBlacklistedAsync(string token);
    }
}