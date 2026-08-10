namespace ATLASDomain.Entities;

public class Course
{
    public int CourseID { get; set; }

    public string C_Name { get; set; } = null!;

    public int Credit_Hours { get; set; }

    public bool IsPractical { get; set; }

    public ICollection<CourseOffering> CourseOfferings { get; set; } = new List<CourseOffering>();
}