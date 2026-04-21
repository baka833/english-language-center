using EnglishCenter.API.DTOs;
using EnglishCenter.API.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EnglishCenter.API.Controllers;

[ApiController]
[Route("api/teacher")]
[Authorize(Roles = "Teacher")]
public sealed class TeacherController : ControllerBase
{
    private readonly ITeacherManagementService _teacherManagementService;

    public TeacherController(ITeacherManagementService teacherManagementService)
    {
        _teacherManagementService = teacherManagementService;
    }

    [HttpGet("classes")]
    public async Task<IActionResult> GetAssignedClasses(CancellationToken cancellationToken)
    {
        return await ExecuteAsync(async () =>
        {
            var teacherId = GetAuthenticatedTeacherId();
            return Ok(await _teacherManagementService.GetAssignedClassesAsync(teacherId, cancellationToken));
        });
    }

    [HttpGet("schedule")]
    public async Task<IActionResult> GetSchedule([FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate, [FromQuery] int? month, [FromQuery] int? year, CancellationToken cancellationToken)
    {
        return await ExecuteAsync(async () =>
        {
            var teacherId = GetAuthenticatedTeacherId();
            return Ok(await _teacherManagementService.GetTeacherScheduleAsync(teacherId, fromDate, toDate, month, year, cancellationToken));
        });
    }

    [HttpGet("classes/{classId:int}/students")]
    public async Task<IActionResult> GetStudentsByClass(int classId, CancellationToken cancellationToken)
    {
        return await ExecuteAsync(async () =>
        {
            var teacherId = GetAuthenticatedTeacherId();
            var students = await _teacherManagementService.GetStudentsByClassAsync(teacherId, classId, cancellationToken);
            return students is null ? NotFound() : Ok(students);
        });
    }

    [HttpGet("classes/{classId:int}/attendance")]
    public async Task<IActionResult> GetAttendanceByDate(int classId, [FromQuery] DateOnly attendanceDate, [FromQuery] int scheduleId, CancellationToken cancellationToken)
    {
        return await ExecuteAsync(async () =>
        {
            var teacherId = GetAuthenticatedTeacherId();
            var attendance = await _teacherManagementService.GetAttendanceByDateAsync(teacherId, classId, attendanceDate, scheduleId, cancellationToken);
            return attendance is null ? NotFound() : Ok(attendance);
        });
    }

    [HttpGet("classes/{classId:int}/attendance-slots")]
    public async Task<IActionResult> GetAttendanceSlots(int classId, [FromQuery] DateOnly attendanceDate, CancellationToken cancellationToken)
    {
        return await ExecuteAsync(async () =>
        {
            var teacherId = GetAuthenticatedTeacherId();
            var slots = await _teacherManagementService.GetAttendanceSlotsAsync(teacherId, classId, attendanceDate, cancellationToken);
            return slots is null ? NotFound() : Ok(slots);
        });
    }

    [HttpPost("classes/{classId:int}/attendance-checkin")]
    public async Task<IActionResult> CheckInAttendance(int classId, [FromBody] TeacherAttendanceCheckInRequest request, CancellationToken cancellationToken)
    {
        return await ExecuteAsync(async () =>
        {
            var teacherId = GetAuthenticatedTeacherId();
            var slot = await _teacherManagementService.CheckInAttendanceSlotAsync(teacherId, classId, request, cancellationToken);
            return slot is null ? NotFound() : Ok(slot);
        });
    }

    [HttpPut("classes/{classId:int}/attendance")]
    public async Task<IActionResult> UpsertAttendance(int classId, [FromBody] UpsertAttendanceRequest request, CancellationToken cancellationToken)
    {
        return await ExecuteAsync(async () =>
        {
            var teacherId = GetAuthenticatedTeacherId();
            var attendance = await _teacherManagementService.UpsertAttendanceAsync(teacherId, classId, request, cancellationToken);
            return attendance is null ? NotFound() : Ok(attendance);
        });
    }

    [HttpGet("classes/{classId:int}/attendance-summary")]
    public async Task<IActionResult> GetAttendanceSummary(int classId, CancellationToken cancellationToken)
    {
        return await ExecuteAsync(async () =>
        {
            var teacherId = GetAuthenticatedTeacherId();
            var summary = await _teacherManagementService.GetAttendanceSummaryAsync(teacherId, classId, cancellationToken);
            return summary is null ? NotFound() : Ok(summary);
        });
    }

    [HttpGet("classes/{classId:int}/grade-components")]
    public async Task<IActionResult> GetGradeComponents(int classId, CancellationToken cancellationToken)
    {
        return await ExecuteAsync(async () =>
        {
            var teacherId = GetAuthenticatedTeacherId();
            var components = await _teacherManagementService.GetGradeComponentsAsync(teacherId, classId, cancellationToken);
            return components is null ? NotFound() : Ok(components);
        });
    }

    [HttpPost("classes/{classId:int}/grade-components")]
    public async Task<IActionResult> CreateGradeComponent(int classId, [FromBody] UpsertGradeComponentRequest request, CancellationToken cancellationToken)
    {
        return await ExecuteAsync(async () =>
        {
            var teacherId = GetAuthenticatedTeacherId();
            var component = await _teacherManagementService.CreateGradeComponentAsync(teacherId, classId, request, cancellationToken);
            return component is null ? NotFound() : Ok(component);
        });
    }

    [HttpPut("classes/{classId:int}/grade-components/{componentId:int}")]
    public async Task<IActionResult> UpdateGradeComponent(int classId, int componentId, [FromBody] UpsertGradeComponentRequest request, CancellationToken cancellationToken)
    {
        return await ExecuteAsync(async () =>
        {
            var teacherId = GetAuthenticatedTeacherId();
            var component = await _teacherManagementService.UpdateGradeComponentAsync(teacherId, classId, componentId, request, cancellationToken);
            return component is null ? NotFound() : Ok(component);
        });
    }

    [HttpDelete("classes/{classId:int}/grade-components/{componentId:int}")]
    public async Task<IActionResult> DeleteGradeComponent(int classId, int componentId, CancellationToken cancellationToken)
    {
        return await ExecuteAsync(async () =>
        {
            var teacherId = GetAuthenticatedTeacherId();
            var deleted = await _teacherManagementService.DeleteGradeComponentAsync(teacherId, classId, componentId, cancellationToken);
            return deleted ? NoContent() : NotFound();
        });
    }

    [HttpGet("classes/{classId:int}/grade-components/{componentId:int}/grades")]
    public async Task<IActionResult> GetGrades(int classId, int componentId, CancellationToken cancellationToken)
    {
        return await ExecuteAsync(async () =>
        {
            var teacherId = GetAuthenticatedTeacherId();
            var grades = await _teacherManagementService.GetGradesAsync(teacherId, classId, componentId, cancellationToken);
            return grades is null ? NotFound() : Ok(grades);
        });
    }

    [HttpPut("classes/{classId:int}/grade-components/{componentId:int}/grades")]
    public async Task<IActionResult> UpsertGrade(int classId, int componentId, [FromBody] UpsertGradeRequest request, CancellationToken cancellationToken)
    {
        return await ExecuteAsync(async () =>
        {
            var teacherId = GetAuthenticatedTeacherId();
            var grade = await _teacherManagementService.UpsertGradeAsync(teacherId, classId, componentId, request, cancellationToken);
            return grade is null ? NotFound() : Ok(grade);
        });
    }

    [HttpGet("applications")]
    public async Task<IActionResult> GetApplications(CancellationToken cancellationToken)
    {
        return await ExecuteAsync(async () =>
        {
            var teacherId = GetAuthenticatedTeacherId();
            return Ok(await _teacherManagementService.GetApplicationsAsync(teacherId, cancellationToken));
        });
    }

    [HttpPost("applications")]
    public async Task<IActionResult> CreateApplication([FromBody] CreateTeacherApplicationRequest request, CancellationToken cancellationToken)
    {
        return await ExecuteAsync(async () =>
        {
            var teacherId = GetAuthenticatedTeacherId();
            return Ok(await _teacherManagementService.CreateApplicationAsync(teacherId, request, cancellationToken));
        });
    }

    private int GetAuthenticatedTeacherId()
    {
        var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(claimValue, out var authenticatedTeacherId))
        {
            throw new UnauthorizedAccessException("The authenticated user identifier is invalid.");
        }

        return authenticatedTeacherId;
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
        catch (UnauthorizedAccessException exception)
        {
            return Unauthorized(new { message = exception.Message });
        }
    }
}