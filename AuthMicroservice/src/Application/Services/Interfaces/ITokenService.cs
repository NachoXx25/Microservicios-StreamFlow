using AuthMicroservice.src.Domain.Models;

namespace AuthMicroservice.src.Application.Services.Interfaces
{
    public interface ITokenService
    {
        Task<string> CreateToken(User user);
    }
}