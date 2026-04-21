using EnglishCenter.API.DTOs.Auth;
using EnglishCenter.API.Models;
using EnglishCenter.API.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IJwtService _jwt;
        private readonly EnglishCenterDbContext _db;
        private readonly IPasswordHasher<User> _hasher;

        public AuthController(IJwtService jwt, EnglishCenterDbContext db, IPasswordHasher<User> hasher)
        {
            _jwt = jwt;
            _db = db;
            _hasher = hasher;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
            if (user == null)
                return Unauthorized(new { message = "Invalid username or password." });

            PasswordVerificationResult result;
            try
            {
                result = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
            }
            catch (FormatException)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            if (result == PasswordVerificationResult.Failed)
                return Unauthorized(new { message = "Invalid username or password." });

            if (user.IsActive == false)
                return Unauthorized(new { message = "Account is disabled." });

            var accessToken = _jwt.GenerateAccessToken(user);

            return Ok(new AuthResponseDto
            {
                AccessToken = accessToken,
                Fullname = user.Fullname,
                Role = user.Role
            });
        }
    }
}
