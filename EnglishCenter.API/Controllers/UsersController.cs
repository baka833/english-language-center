using EnglishCenter.API.DTOs.Users;
using EnglishCenter.API.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EnglishCenter.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UsersController : ControllerBase
{
    private readonly IUserProfileService _userProfileService;

    public UsersController(IUserProfileService userProfileService)
    {
        _userProfileService = userProfileService;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var profile = await _userProfileService.GetProfileAsync(userId, cancellationToken);
        return profile is null ? NotFound(new { message = "User not found." }) : Ok(profile);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Invalid request data." });
        }

        var userId = GetUserId();
        var (success, error) = await _userProfileService.UpdateProfileAsync(userId, request, cancellationToken);
        return success ? Ok() : BadRequest(new { message = error });
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Invalid request data." });
        }

        var userId = GetUserId();
        var (success, error) = await _userProfileService.ChangePasswordAsync(userId, request, cancellationToken);
        return success ? Ok() : BadRequest(new { message = error });
    }

    private int GetUserId()
    {
        var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(claimValue, out var userId))
        {
            throw new UnauthorizedAccessException("The authenticated user identifier is invalid.");
        }

        return userId;
    }
}
