namespace EnglishCenter.API.DTOs.Admin;

public class UserSummaryDto
{
    public int UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Fullname { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Role { get; set; }

    public bool CanManageGradeComponents { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }
}

public sealed class UserDetailDto : UserSummaryDto
{
    public DateOnly? Dob { get; set; }

    public string? Gender { get; set; }
}

public sealed class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;

    public string Fullname { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Gender { get; set; }

    public DateOnly? Dob { get; set; }

    public string Role { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public bool CanManageGradeComponents { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class UpdateUserRequest
{
    public string Fullname { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Gender { get; set; }

    public DateOnly? Dob { get; set; }

    public string Role { get; set; } = string.Empty;

    public bool? CanManageGradeComponents { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class LockUserRequest
{
    public bool IsActive { get; set; }
}

public sealed class UpdateTeacherGradeComponentPermissionRequest
{
    public bool CanManageGradeComponents { get; set; }
}

public sealed class ResetPasswordResultDto
{
    public int UserId { get; set; }

    public string NewPassword { get; set; } = string.Empty;
}