using System;
using System.Collections.Generic;

namespace EnglishCenter.API.Models;

public partial class Schedule
{
    public int ScheduleId { get; set; }

    public int ClassId { get; set; }

    public DateOnly? ScheduleDate { get; set; }

    public int? DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string? Room { get; set; }

    public virtual Class Class { get; set; } = null!;
}
