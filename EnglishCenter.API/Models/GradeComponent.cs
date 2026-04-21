namespace EnglishCenter.API.Models;

public partial class GradeComponent
{
    public int ComponentId { get; set; }

    public int ClassId { get; set; }

    public string ComponentName { get; set; } = null!;

    public decimal Weight { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
}
