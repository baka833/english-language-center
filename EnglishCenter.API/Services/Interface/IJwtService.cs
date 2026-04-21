using EnglishCenter.API.Models;
using System.Security.Claims;

namespace EnglishCenter.API.Services.Interface
{
    public interface IJwtService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
        void SaveRefreshToken(int userId, string refreshToken);
        bool ValidateRefreshToken(int userId, string refreshToken);
        void RevokeRefreshToken(int userId);
    }
}
