using EnglishCenter.API.DTOs;
using EnglishCenter.API.Models;
using EnglishCenter.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
            if (result == PasswordVerificationResult.Failed)
                return Unauthorized(new { message = "Invalid username or password." });

            if (user.IsActive == false)
                return Unauthorized(new { message = "Account is disabled." });

            var accessToken = _jwt.GenerateAccessToken(user);
            var refreshToken = _jwt.GenerateRefreshToken();
            _jwt.SaveRefreshToken(user.UserId, refreshToken);

            return Ok(new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Fullname = user.Fullname,
                Role = user.Role
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            if (await _db.Users.AnyAsync(u => u.Username == dto.Username))
                return Conflict(new { message = "Username already exists." });

            var user = new User
            {
                Username = dto.Username,
                Fullname = dto.Fullname,
                Email = dto.Email,
                Gender = dto.Gender,
                Dob = dto.Dob,
                Role = "Student",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                PasswordHash = string.Empty
            };
            user.PasswordHash = _hasher.HashPassword(user, dto.Password);

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var accessToken = _jwt.GenerateAccessToken(user);
            var refreshToken = _jwt.GenerateRefreshToken();
            _jwt.SaveRefreshToken(user.UserId, refreshToken);

            return CreatedAtAction(nameof(Login), new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Fullname = user.Fullname,
                Role = user.Role
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto dto)
        {
            var accessToken = Request.Headers.Authorization.FirstOrDefault()?.Replace("Bearer ", "");
            if (string.IsNullOrEmpty(accessToken))
                return BadRequest(new { message = "Access token is required." });

            var principal = _jwt.GetPrincipalFromExpiredToken(accessToken);
            if (principal == null)
                return Unauthorized(new { message = "Invalid access token." });

            var userIdClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { message = "Invalid token claims." });

            if (!_jwt.ValidateRefreshToken(userId, dto.RefreshToken))
                return Unauthorized(new { message = "Invalid or expired refresh token." });

            var user = await _db.Users.FindAsync(userId);
            if (user == null || user.IsActive == false)
                return Unauthorized(new { message = "User not found or disabled." });

            var newAccessToken = _jwt.GenerateAccessToken(user);
            var newRefreshToken = _jwt.GenerateRefreshToken();
            _jwt.SaveRefreshToken(userId, newRefreshToken);

            return Ok(new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                Fullname = user.Fullname,
                Role = user.Role
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out var userId))
                _jwt.RevokeRefreshToken(userId);

            return NoContent();
        }
    }
}
