namespace EnglishCenter.API.Models;

public partial class Attendance
{
    public int AttendanceId { get; set; }

    public int ScheduleId { get; set; }

    public int StudentId { get; set; }

    public DateOnly AttendanceDate { get; set; }

    public string? Status { get; set; }

    public string? Note { get; set; }

    public virtual Schedule Schedule { get; set; } = null!;

    public virtual User Student { get; set; } = null!;
}
