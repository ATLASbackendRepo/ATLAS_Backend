namespace ATLASDomain.Entities;

/// <summary>
/// Enrollment.
/// </summary>
public class Enrollment
{
    /// <summary>
    /// Enrollment ID.
    /// </summary>
    public int EnrollmentID { get; set; }

    /// <summary>
    /// Student ID.
    /// </summary>
    public int StudentID { get; set; }

    /// <summary>
    /// Created By User ID.
    /// </summary>
    public int CreatedByUserID { get; set; }
    /// <summary>
    /// Enrollment Date.
    /// </summary>
    public DateTime EnrollmentDate { get; set; }

    /// <summary>
    /// Course Offering ID.
    /// </summary>
    public int CourseOfferingID { get; set; }

    /// <summary>
    /// Student.
    /// </summary>
    public Student Student { get; set; } = null!;

    /// <summary>
    /// Created By Super Admin.
    /// </summary>
    public User CreatedBySuperAdmin { get; set; } = null!;

    /// <summary>
    /// Course Offering.
    /// </summary>
    public CourseOffering CourseOffering { get; set; } = null!;
    //public CourseGrade?  CourseGrade { get; set; } = null!;

    /// <summary>
    /// Course Grades.
    /// </summary>
    public ICollection<CourseGrade> CourseGrades { get; set; } = new List<CourseGrade>();

}
