namespace EnglishCenter.API.Models;

public partial class Grade
{
    public int GradeId { get; set; }

    public int ComponentId { get; set; }

    public int StudentId { get; set; }

    public decimal? GradeValue { get; set; }

    public string? TeacherComment { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual GradeComponent Component { get; set; } = null!;

    public virtual User Student { get; set; } = null!;
}
