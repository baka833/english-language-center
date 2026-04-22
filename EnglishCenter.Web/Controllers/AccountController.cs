using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EnglishCenter.Web.Models.Account;
using EnglishCenter.Web.Models.Auth;
using EnglishCenter.Web.Services.Impl;
using EnglishCenter.Web.Services.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Web.Controllers;

public sealed class AccountController : Controller
{
    private readonly IAuthApiClient _authApiClient;
    private readonly IUserApiClient _userApiClient;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IAuthApiClient authApiClient, IUserApiClient userApiClient, ILogger<AccountController> logger)
    {
        _authApiClient = authApiClient;
        _userApiClient = userApiClient;
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

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile(CancellationToken cancellationToken)
    {
        try
        {
            var profile = await _userApiClient.GetProfileAsync(cancellationToken);
            if (profile is null)
            {
                TempData["ErrorMessage"] = "Profile could not be loaded.";
                return RedirectToDefaultWorkspace();
            }

            return View(new ProfileViewModel { Profile = profile });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to load profile.");
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToDefaultWorkspace();
        }
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> EditProfile(CancellationToken cancellationToken)
    {
        try
        {
            var profile = await _userApiClient.GetProfileAsync(cancellationToken);
            if (profile is null)
            {
                TempData["ErrorMessage"] = "Profile could not be loaded.";
                return RedirectToAction(nameof(Profile));
            }

            return View(new EditProfileViewModel
            {
                Username = profile.Username,
                Role = profile.Role,
                Fullname = profile.Fullname,
                Gender = profile.Gender,
                Dob = profile.Dob
            });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to load profile for editing.");
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToAction(nameof(Profile));
        }
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(EditProfileViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var request = new UpdateProfileApiRequest
        {
            Fullname = model.Fullname,
            Gender = model.Gender,
            Dob = model.Dob
        };

        var (success, error) = await _userApiClient.UpdateProfileAsync(request, cancellationToken);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Update failed.");
            return View(model);
        }

        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["SuccessMessage"] = "Profile updated. Please sign in again.";
        return RedirectToAction(nameof(Login));
    }

    [Authorize]
    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var request = new ChangePasswordApiRequest
        {
            CurrentPassword = model.CurrentPassword,
            NewPassword = model.NewPassword,
            ConfirmPassword = model.ConfirmPassword
        };

        var (success, error) = await _userApiClient.ChangePasswordAsync(request, cancellationToken);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Password change failed.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Password changed successfully.";
        return RedirectToAction(nameof(Profile));
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
