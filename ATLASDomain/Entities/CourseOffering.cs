namespace ATLASDomain.Entities;

/// <summary>
/// Course Offering.
/// </summary>
public class CourseOffering
{
    /// <summary>
    /// Course Offering ID.
    /// </summary>
    public int CourseOfferingID { get; set; }

    /// <summary>
    /// Semester ID.
    /// </summary>
    public int SemesterID { get; set; }

    /// <summary>
    /// Course ID.
    /// </summary>
    public int CourseID { get; set; }

    /// <summary>
    /// Institution ID.
    /// </summary>
    public int? InstitutionID { get; set; }

    /// <summary>
    /// Institution.
    /// </summary>
    public Institution? Institution { get; set; }

    /// <summary>
    /// Semester.
    /// </summary>
    public Semester Semester { get; set; } = null!;

    /// <summary>
    /// Course.
    /// </summary>
    public Course Course { get; set; } = null!;

    /// <summary>
    /// Enrollments.
    /// </summary>
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    /// <summary>
    /// Course Offering Doctors.
    /// </summary>
    public ICollection<CourseOfferingDoctor> CourseOfferingDoctors { get; set; } = new List<CourseOfferingDoctor>();

    /// <summary>
    /// Course Offering TAs.
    /// </summary>
    public ICollection<CourseOfferingTA> CourseOfferingTAs { get; set; } = new List<CourseOfferingTA>();
    /// <summary>
    /// Assessment Modules.
    /// </summary>
    public ICollection<AssessmentModule> AssessmentModules { get; set; } = new List<AssessmentModule>();
    /// <summary>
    /// Attendance Sessions.
    /// </summary>
    public ICollection<AttendanceSession> AttendanceSessions { get; set; } = new List<AttendanceSession>();
}
