namespace EnglishCenter.API.DTOs.Auth
{
    public class LoginRequestDto
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class RegisterRequestDto
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Fullname { get; set; } = null!;
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public DateOnly? Dob { get; set; }
    }

    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = null!;
        public string Fullname { get; set; } = null!;
        public string? Role { get; set; }
    }
}
