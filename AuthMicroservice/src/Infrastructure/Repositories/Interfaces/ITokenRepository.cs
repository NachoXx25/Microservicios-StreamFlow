namespace AuthMicroservice.src.Infrastructure.Repositories.Interfaces
{
    public interface ITokenRepository
    {
        /// <summary>
        /// Verifica si un token existe en la base de datos.
        /// </summary>
        /// <param name="jti">Identificador único del token.</param>
        /// <returns>True si el token existe, de lo contrario false.</returns>
        Task<bool> VerifyIfTokenExists(string jti);

        /// <summary>
        /// Agrega un token a la lista negra.
        /// </summary>
        /// <param name="jti">Identificador único del token.</param>
        /// <returns>True si el token fue agregado, de lo contrario false.</returns>
        Task<bool> AddTokenToBlacklist(string jti);

        /// <summary>
        /// Valida un token JWT y verifica si está en la lista negra.
        /// </summary>
        /// <param name="token">Token a validar.</param>
        /// <returns>Respuesta de validación del token.</returns>
        Task<bool> IsTokenBlacklistedAsync(string token);
    }
}