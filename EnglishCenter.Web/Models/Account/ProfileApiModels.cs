namespace EnglishCenter.Web.Models.Account;

public sealed class UserProfileApiModel
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Fullname { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Gender { get; set; }
    public DateOnly? Dob { get; set; }
    public string? Role { get; set; }
}

public sealed class UpdateProfileApiRequest
{
    public string Fullname { get; set; } = string.Empty;
    public string? Gender { get; set; }
    public DateOnly? Dob { get; set; }
}

public sealed class ChangePasswordApiRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
