namespace EnglishCenter.API.Models;

public partial class ClassStudent
{
    public int ClassId { get; set; }

    public int StudentId { get; set; }

    public DateTime? EnrollmentDate { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual User Student { get; set; } = null!;
}
