using System;
using System.Collections.Generic;

namespace EnglishCenter.API.Models;

public partial class Attendance
{
    public int AttendanceId { get; set; }

    public int ClassId { get; set; }

    public int StudentId { get; set; }

    public DateOnly AttendanceDate { get; set; }

    public string? Status { get; set; }

    public string? Note { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual User Student { get; set; } = null!;
}
