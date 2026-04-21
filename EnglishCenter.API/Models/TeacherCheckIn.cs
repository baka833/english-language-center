namespace EnglishCenter.API.Models;

public partial class TeacherCheckIn
{
    public int CheckInId { get; set; }

    public int TeacherId { get; set; }

    public int ScheduleId { get; set; }

    public DateOnly AttendanceDate { get; set; }

    public DateTime CheckedInAt { get; set; }

    public virtual User Teacher { get; set; } = null!;

    public virtual Schedule Schedule { get; set; } = null!;
}
