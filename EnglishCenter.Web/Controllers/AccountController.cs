using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EnglishCenter.Web.Models.Auth;
using EnglishCenter.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Web.Controllers;

public sealed class AccountController : Controller
{
    private readonly IAuthApiClient _authApiClient;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IAuthApiClient authApiClient, ILogger<AccountController> logger)
    {
        _authApiClient = authApiClient;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToDefaultWorkspace();
        }

        return View(new LoginPageViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login([Bind(Prefix = "Form")] LoginForm form, string? returnUrl, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(form.Username) || string.IsNullOrWhiteSpace(form.Password))
        {
            TempData["ErrorMessage"] = "Username and password are required.";
            return View(new LoginPageViewModel { Form = form, ReturnUrl = returnUrl });
        }

        try
        {
            var auth = await _authApiClient.LoginAsync(form, cancellationToken);
            var principal = BuildPrincipal(auth);
            var role = principal.FindFirstValue(ClaimTypes.Role);

            HttpContext.Session.SetString(AuthSessionKeys.AccessToken, auth.AccessToken);
            HttpContext.Session.SetString(AuthSessionKeys.RefreshToken, auth.RefreshToken);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToDefaultWorkspace(role);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Login failed for user {Username}.", form.Username);
            TempData["ErrorMessage"] = exception.Message;
            return View(new LoginPageViewModel { Form = new LoginForm { Username = form.Username }, ReturnUrl = returnUrl });
        }
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["SuccessMessage"] = "Signed out.";
        return RedirectToAction(nameof(Login));
    }

    [Authorize]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private ClaimsPrincipal BuildPrincipal(AuthResponseModel auth)
    {
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(auth.AccessToken);
        var userId = jwt.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier || claim.Type == JwtRegisteredClaimNames.Sub)?.Value
            ?? throw new InvalidOperationException("Access token is missing the user identifier claim.");
        var username = jwt.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name || claim.Type == JwtRegisteredClaimNames.UniqueName)?.Value
            ?? formFallback(auth.Fullname);
        var role = jwt.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role || claim.Type == "role")?.Value
            ?? auth.Role
            ?? throw new InvalidOperationException("Access token is missing the role claim.");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Role, role),
            new("full_name", auth.Fullname)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);

        static string formFallback(string fullname)
        {
            return string.IsNullOrWhiteSpace(fullname) ? "user" : fullname.Trim();
        }
    }

    private IActionResult RedirectToDefaultWorkspace(string? role = null)
    {
        if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) || User.IsInRole("Admin"))
        {
            return RedirectToAction("Users", "Admin");
        }

        if (string.Equals(role, "Teacher", StringComparison.OrdinalIgnoreCase) || User.IsInRole("Teacher"))
        {
            return RedirectToAction("Classes", "Teacher");
        }

        return RedirectToAction("Index", "Home");
    }
}
