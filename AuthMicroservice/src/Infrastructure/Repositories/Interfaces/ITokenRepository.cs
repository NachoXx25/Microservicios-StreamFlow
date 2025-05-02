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
    }
}