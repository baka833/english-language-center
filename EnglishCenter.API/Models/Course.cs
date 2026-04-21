namespace EnglishCenter.API.Models;

public partial class Course
{
    public int CourseId { get; set; }

    public string CourseName { get; set; } = null!;

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public int? TotalSlots { get; set; }

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
}
