namespace ATLASDomain.Entities;

/// <summary>
/// Semester.
/// </summary>
public class Semester
{
    /// <summary>
    /// Semester ID.
    /// </summary>
    public int SemesterID { get; set; }

    /// <summary>
    /// Semester Name.
    /// </summary>
    public string SemesterName { get; set; } = null!;

    /// <summary>
    /// Academic Year.
    /// </summary>
    public string AcademicYear { get; set; } = null!;

    /// <summary>
    /// Course Offerings.
    /// </summary>
    public ICollection<CourseOffering> CourseOfferings { get; set; } = new List<CourseOffering>();
}
