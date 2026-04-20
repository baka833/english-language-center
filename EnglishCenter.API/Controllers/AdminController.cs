using EnglishCenter.API.DTOs;
using EnglishCenter.API.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.API.Controllers;

[ApiController]
[Route("api/admin")]
public sealed class AdminController : ControllerBase
{
    private readonly IAdminManagementService _adminManagementService;

    public AdminController(IAdminManagementService adminManagementService)
    {
        _adminManagementService = adminManagementService;
    }

    [HttpGet("users")]
    public async Task<ActionResult<IReadOnlyCollection<UserSummaryDto>>> GetUsers([FromQuery] string? role, [FromQuery] bool? isActive, CancellationToken cancellationToken)
    {
        var users = await _adminManagementService.GetUsersAsync(role, isActive, cancellationToken);
        return Ok(users);
    }

    [HttpGet("users/{userId:int}")]
    public async Task<ActionResult<UserDetailDto>> GetUserById(int userId, CancellationToken cancellationToken)
    {
        var user = await _adminManagementService.GetUserByIdAsync(userId, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost("users")]
    public async Task<ActionResult<UserDetailDto>> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        return await ExecuteResultAsync<UserDetailDto>(async () =>
        {
            var createdUser = await _adminManagementService.CreateUserAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetUserById), new { userId = createdUser.UserId }, createdUser);
        });
    }

    [HttpPut("users/{userId:int}")]
    public async Task<ActionResult<UserDetailDto>> UpdateUser(int userId, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        return await ExecuteResultAsync<UserDetailDto>(async () =>
        {
            var updatedUser = await _adminManagementService.UpdateUserAsync(userId, request, cancellationToken);
            return updatedUser is null ? NotFound() : Ok(updatedUser);
        });
    }

    [HttpPatch("users/{userId:int}/activation")]
    public async Task<ActionResult<UserDetailDto>> SetUserActivation(int userId, [FromBody] LockUserRequest request, CancellationToken cancellationToken)
    {
        return await ExecuteResultAsync<UserDetailDto>(async () =>
        {
            var updatedUser = await _adminManagementService.SetUserActivationAsync(userId, request.IsActive, cancellationToken);
            return updatedUser is null ? NotFound() : Ok(updatedUser);
        });
    }

    [HttpPatch("users/{userId:int}/grade-component-permission")]
    public async Task<ActionResult<UserDetailDto>> SetTeacherGradeComponentPermission(int userId, [FromBody] UpdateTeacherGradeComponentPermissionRequest request, CancellationToken cancellationToken)
    {
        return await ExecuteResultAsync<UserDetailDto>(async () =>
        {
            var updatedUser = await _adminManagementService.UpdateTeacherGradeComponentPermissionAsync(userId, request.CanManageGradeComponents, cancellationToken);
            return updatedUser is null ? NotFound() : Ok(updatedUser);
        });
    }

    [HttpPost("users/{userId:int}/reset-password")]
    public async Task<ActionResult<ResetPasswordResultDto>> ResetPassword(int userId, CancellationToken cancellationToken)
    {
        return await ExecuteResultAsync<ResetPasswordResultDto>(async () =>
        {
            var resetPasswordResult = await _adminManagementService.ResetPasswordAsync(userId, cancellationToken);
            return resetPasswordResult is null ? NotFound() : Ok(resetPasswordResult);
        });
    }

    [HttpGet("courses")]
    public async Task<ActionResult<IReadOnlyCollection<CourseDto>>> GetCourses(CancellationToken cancellationToken)
    {
        var courses = await _adminManagementService.GetCoursesAsync(cancellationToken);
        return Ok(courses);
    }

    [HttpGet("courses/{courseId:int}")]
    public async Task<ActionResult<CourseDto>> GetCourseById(int courseId, CancellationToken cancellationToken)
    {
        var course = await _adminManagementService.GetCourseByIdAsync(courseId, cancellationToken);
        return course is null ? NotFound() : Ok(course);
    }

    [HttpPost("courses")]
    public async Task<ActionResult<CourseDto>> CreateCourse([FromBody] UpsertCourseRequest request, CancellationToken cancellationToken)
    {
        return await ExecuteResultAsync<CourseDto>(async () =>
        {
            var course = await _adminManagementService.CreateCourseAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetCourseById), new { courseId = course.CourseId }, course);
        });
    }

    [HttpPut("courses/{courseId:int}")]
    public async Task<ActionResult<CourseDto>> UpdateCourse(int courseId, [FromBody] UpsertCourseRequest request, CancellationToken cancellationToken)
    {
        return await ExecuteResultAsync<CourseDto>(async () =>
        {
            var course = await _adminManagementService.UpdateCourseAsync(courseId, request, cancellationToken);
            return course is null ? NotFound() : Ok(course);
        });
    }

    [HttpDelete("courses/{courseId:int}")]
    public async Task<IActionResult> DeleteCourse(int courseId, CancellationToken cancellationToken)
    {
        return await ExecuteActionAsync(async () =>
        {
            var deleted = await _adminManagementService.DeleteCourseAsync(courseId, cancellationToken);
            return deleted ? NoContent() : NotFound();
        });
    }

    [HttpGet("classes")]
    public async Task<ActionResult<IReadOnlyCollection<ClassListDto>>> GetClasses(CancellationToken cancellationToken)
    {
        var classes = await _adminManagementService.GetClassesAsync(cancellationToken);
        return Ok(classes);
    }

    [HttpGet("classes/{classId:int}")]
    public async Task<ActionResult<ClassDetailDto>> GetClassById(int classId, CancellationToken cancellationToken)
    {
        var classDetail = await _adminManagementService.GetClassByIdAsync(classId, cancellationToken);
        return classDetail is null ? NotFound() : Ok(classDetail);
    }

    [HttpPost("classes")]
    public async Task<ActionResult<ClassDetailDto>> CreateClass([FromBody] CreateClassRequest request, CancellationToken cancellationToken)
    {
        return await ExecuteResultAsync<ClassDetailDto>(async () =>
        {
            var classDetail = await _adminManagementService.CreateClassAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetClassById), new { classId = classDetail.ClassId }, classDetail);
        });
    }

    [HttpPut("classes/{classId:int}")]
    public async Task<ActionResult<ClassDetailDto>> UpdateClass(int classId, [FromBody] UpdateClassRequest request, CancellationToken cancellationToken)
    {
        return await ExecuteResultAsync<ClassDetailDto>(async () =>
        {
            var classDetail = await _adminManagementService.UpdateClassAsync(classId, request, cancellationToken);
            return classDetail is null ? NotFound() : Ok(classDetail);
        });
    }

    [HttpDelete("classes/{classId:int}")]
    public async Task<IActionResult> DeleteClass(int classId, CancellationToken cancellationToken)
    {
        return await ExecuteActionAsync(async () =>
        {
            var deleted = await _adminManagementService.DeleteClassAsync(classId, cancellationToken);
            return deleted ? NoContent() : NotFound();
        });
    }

    [HttpPatch("classes/{classId:int}/grade-component-permission")]
    public async Task<ActionResult<ClassDetailDto>> SetClassGradeComponentPermission(int classId, [FromBody] UpdateClassGradeComponentPermissionRequest request, CancellationToken cancellationToken)
    {
        return await ExecuteResultAsync<ClassDetailDto>(async () =>
        {
            var classDetail = await _adminManagementService.UpdateClassGradeComponentPermissionAsync(classId, request.AllowTeacherGradeComponentManagement, cancellationToken);
            return classDetail is null ? NotFound() : Ok(classDetail);
        });
    }

    [HttpPut("classes/{classId:int}/teacher")]
    public async Task<ActionResult<ClassDetailDto>> AssignTeacher(int classId, [FromBody] AssignTeacherRequest request, CancellationToken cancellationToken)
    {
        return await ExecuteResultAsync<ClassDetailDto>(async () =>
        {
            var classDetail = await _adminManagementService.AssignTeacherAsync(classId, request.TeacherId, cancellationToken);
            return classDetail is null ? NotFound() : Ok(classDetail);
        });
    }

    [HttpPost("classes/{classId:int}/students")]
    public async Task<ActionResult<IReadOnlyCollection<ClassStudentDto>>> AssignStudents(int classId, [FromBody] AssignStudentsRequest request, CancellationToken cancellationToken)
    {
        return await ExecuteResultAsync<IReadOnlyCollection<ClassStudentDto>>(async () =>
        {
            var students = await _adminManagementService.AssignStudentsAsync(classId, request.StudentIds, cancellationToken);
            return students is null ? NotFound() : Ok(students);
        });
    }

    [HttpDelete("classes/{classId:int}/students/{studentId:int}")]
    public async Task<IActionResult> RemoveStudent(int classId, int studentId, CancellationToken cancellationToken)
    {
        return await ExecuteActionAsync(async () =>
        {
            var removed = await _adminManagementService.RemoveStudentAsync(classId, studentId, cancellationToken);
            return removed ? NoContent() : NotFound();
        });
    }

    [HttpGet("classes/{classId:int}/schedule-planner")]
    public async Task<ActionResult<ClassSchedulePlannerDto>> GetSchedulePlanner(int classId, [FromQuery] int? month, [FromQuery] int? year, [FromQuery] int? weekIndex, CancellationToken cancellationToken)
    {
        return await ExecuteResultAsync<ClassSchedulePlannerDto>(async () =>
        {
            var planner = await _adminManagementService.GetClassSchedulePlannerAsync(classId, month, year, weekIndex, cancellationToken);
            return planner is null ? NotFound() : Ok(planner);
        });
    }

    [HttpPost("classes/{classId:int}/schedule-planner")]
    public async Task<ActionResult<ClassSchedulePlannerDto>> SaveSchedulePlanner(int classId, [FromBody] SaveClassSchedulePlannerRequest request, CancellationToken cancellationToken)
    {
        return await ExecuteResultAsync<ClassSchedulePlannerDto>(async () =>
        {
            var planner = await _adminManagementService.SaveClassSchedulePlannerAsync(classId, request, cancellationToken);
            return planner is null ? NotFound() : Ok(planner);
        });
    }

    [HttpPut("classes/{classId:int}/schedules")]
    public async Task<ActionResult<IReadOnlyCollection<ScheduleDto>>> UpdateSchedules(int classId, [FromBody] UpdateClassSchedulesRequest request, CancellationToken cancellationToken)
    {
        return await ExecuteResultAsync<IReadOnlyCollection<ScheduleDto>>(async () =>
        {
            var schedules = await _adminManagementService.UpdateClassSchedulesAsync(classId, request.Schedules, cancellationToken);
            return schedules is null ? NotFound() : Ok(schedules);
        });
    }

    [HttpGet("applications")]
    public async Task<ActionResult<IReadOnlyCollection<ApplicationDto>>> GetApplications([FromQuery] string? status, CancellationToken cancellationToken)
    {
        var applications = await _adminManagementService.GetApplicationsAsync(status, cancellationToken);
        return Ok(applications);
    }

    [HttpPost("applications/{applicationId:int}/response")]
    public async Task<ActionResult<ApplicationDto>> RespondApplication(int applicationId, [FromBody] RespondApplicationRequest request, CancellationToken cancellationToken)
    {
        return await ExecuteResultAsync<ApplicationDto>(async () =>
        {
            var application = await _adminManagementService.RespondApplicationAsync(applicationId, request, cancellationToken);
            return application is null ? NotFound() : Ok(application);
        });
    }

    private async Task<ActionResult<T>> ExecuteResultAsync<T>(Func<Task<ActionResult<T>>> action)
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

    private async Task<IActionResult> ExecuteActionAsync(Func<Task<IActionResult>> action)
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