namespace ATLASDomain.Entities;

public class Semester
{
    public int SemesterID { get; set; }

    public string SemesterName { get; set; } = null!;

    public string AcademicYear { get; set; } = null!;

    public ICollection<CourseOffering> CourseOfferings { get; set; } = new List<CourseOffering>();
}