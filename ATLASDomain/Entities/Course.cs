namespace ATLASDomain.Entities;

/// <summary>
/// Course.
/// </summary>
public class Course
{
    /// <summary>
    /// Course ID.
    /// </summary>
    public int CourseID { get; set; }

    /// <summary>
    /// C_Name.
    /// </summary>
    public string C_Name { get; set; } = null!;

    /// <summary>
    /// Credit_Hours.
    /// </summary>
    public int Credit_Hours { get; set; }

    /// <summary>
    /// Is Practical.
    /// </summary>
    public bool IsPractical { get; set; }

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
