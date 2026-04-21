using System;
using System.Collections.Generic;

namespace EnglishCenter.API.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Fullname { get; set; } = null!;

    public DateOnly? Dob { get; set; }

    public string? Gender { get; set; }

    public string? Email { get; set; }

    public string? Role { get; set; }

    public bool? CanManageGradeComponents { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual ICollection<ClassStudent> ClassStudents { get; set; } = new List<ClassStudent>();

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();

    public virtual ICollection<TeacherCheckIn> TeacherCheckIns { get; set; } = new List<TeacherCheckIn>();
}
