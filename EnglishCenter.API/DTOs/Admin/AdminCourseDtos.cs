namespace EnglishCenter.API.DTOs.Admin;

public sealed class CourseDto
{
    public int CourseId { get; set; }

    public string CourseName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public int? TotalSlots { get; set; }
}

public sealed class UpsertCourseRequest
{
    public string CourseName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public int? TotalSlots { get; set; }
}