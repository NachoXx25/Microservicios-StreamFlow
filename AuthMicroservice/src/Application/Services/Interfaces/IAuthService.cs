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
    }
}