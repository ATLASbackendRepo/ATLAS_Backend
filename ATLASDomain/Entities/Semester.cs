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
    /// Whether grading has been closed for this semester. While true, no grade
    /// may be assigned or imported for a course offering of this semester (MED-07).
    /// </summary>
    public bool IsGradingClosed { get; set; }

    /// <summary>
    /// Institution ID.
    /// </summary>
    public int? InstitutionID { get; set; }

    /// <summary>
    /// Institution.
    /// </summary>
    public Institution? Institution { get; set; }

    /// <summary>
    /// Course Offerings.
    /// </summary>
    public ICollection<CourseOffering> CourseOfferings { get; set; } = new List<CourseOffering>();
}
