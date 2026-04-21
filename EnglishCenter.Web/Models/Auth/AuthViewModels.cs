namespace EnglishCenter.Web.Models.Auth;

public sealed class LoginForm
{
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

public sealed class LoginPageViewModel
{
    public LoginForm Form { get; set; } = new();

    public string? ReturnUrl { get; set; }
}

public sealed class AuthResponseModel
{
    public string AccessToken { get; set; } = string.Empty;

    public string Fullname { get; set; } = string.Empty;

    public string? Role { get; set; }
}
