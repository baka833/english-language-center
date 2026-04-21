using EnglishCenter.API.Models;
using System.Security.Claims;

namespace EnglishCenter.API.Services.Interface
{
    public interface IJwtService
    {
        string GenerateAccessToken(User user);
    }
}
