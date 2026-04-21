using EnglishCenter.API.DTOs;
using EnglishCenter.API.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EnglishCenter.API.Controllers;

[ApiController]
[Route("api/students/{studentId:int}")]
[Authorize(Roles = "Student")]
public sealed class StudentsController : ControllerBase
{
    private readonly IStudentManagementService _studentManagementService;

    public StudentsController(IStudentManagementService studentManagementService)
    {
        _studentManagementService = studentManagementService;
    }

    [HttpGet("schedule")]
    public async Task<IActionResult> GetSchedule(int studentId, [FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate, [FromQuery] int? month, [FromQuery] int? year, CancellationToken cancellationToken)
    {
        var authorizationResult = EnsureAuthorizedStudent(studentId);
        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        return await ExecuteAsync(async () => Ok(await _studentManagementService.GetScheduleAsync(studentId, fromDate, toDate, month, year, cancellationToken)));
    }

    [HttpGet("applications")]
    public async Task<IActionResult> GetApplications(int studentId, CancellationToken cancellationToken)
    {
        var authorizationResult = EnsureAuthorizedStudent(studentId);
        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        return await ExecuteAsync(async () => Ok(await _studentManagementService.GetApplicationsAsync(studentId, cancellationToken)));
    }

    [HttpPost("applications")]
    public async Task<IActionResult> CreateApplication(int studentId, [FromBody] CreateStudentApplicationRequest request, CancellationToken cancellationToken)
    {
        var authorizationResult = EnsureAuthorizedStudent(studentId);
        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        return await ExecuteAsync(async () => Ok(await _studentManagementService.CreateApplicationAsync(studentId, request, cancellationToken)));
    }

    private IActionResult? EnsureAuthorizedStudent(int studentId)
    {
        var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(claimValue, out var authenticatedStudentId))
        {
            return Unauthorized(new { message = "The authenticated user identifier is invalid." });
        }

        if (authenticatedStudentId != studentId)
        {
            return Forbid();
        }

        return null;
    }

    private async Task<IActionResult> ExecuteAsync(Func<Task<IActionResult>> action)
    {
        try
        {
            return await action();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }
}
