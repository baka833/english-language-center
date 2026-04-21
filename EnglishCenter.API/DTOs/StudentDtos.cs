namespace EnglishCenter.API.DTOs;

public sealed class CreateStudentApplicationRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    public string Type { get; set; } = string.Empty;
}

public sealed class StudentScheduleItemDto
{
    public int ScheduleId { get; set; }

    public int ClassId { get; set; }

    public string ClassName { get; set; } = string.Empty;

    public int CourseId { get; set; }

    public string CourseName { get; set; } = string.Empty;

    public DateOnly? ClassStartDate { get; set; }

    public DateOnly? ClassEndDate { get; set; }

    public DateOnly? ScheduleDate { get; set; }

    public int? DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string? Room { get; set; }
}
