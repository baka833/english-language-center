using EnglishCenter.API.Models;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace EnglishCenter.API.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _signingKey;
        private readonly int _accessTokenExpiryMinutes;
        private readonly int _refreshTokenExpiryDays;

        // In-memory store: userId → (refreshToken, expiry)
        private static readonly ConcurrentDictionary<int, (string Token, DateTime Expiry)> _refreshTokens = new();

        public JwtService(IConfiguration config)
        {
            _config = config;
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);
            _signingKey = new SymmetricSecurityKey(key);
            _accessTokenExpiryMinutes = int.Parse(_config["Jwt:AccessTokenExpiryMinutes"]!);
            _refreshTokenExpiryDays = int.Parse(_config["Jwt:RefreshTokenExpiryDays"]!);
        }

        public string GenerateAccessToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(ClaimTypes.Role, user.Role ?? "Student"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_accessTokenExpiryMinutes),
                signingCredentials: new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Audience"],
                IssuerSigningKey = _signingKey,
            };

            try
            {
                return new JwtSecurityTokenHandler().ValidateToken(token, parameters, out _);
            }
            catch
            {
                return null;
            }
        }

        public void SaveRefreshToken(int userId, string refreshToken)
        {
            _refreshTokens[userId] = (refreshToken, DateTime.UtcNow.AddDays(_refreshTokenExpiryDays));
        }

        public bool ValidateRefreshToken(int userId, string refreshToken)
        {
            if (!_refreshTokens.TryGetValue(userId, out var entry))
                return false;

            return entry.Token == refreshToken && entry.Expiry > DateTime.UtcNow;
        }

        public void RevokeRefreshToken(int userId)
        {
            _refreshTokens.TryRemove(userId, out _);
        }
    }
}
