namespace ATLASDomain.Entities;

/// <summary>
/// Course Offering Doctor.
/// </summary>
public class CourseOfferingDoctor
{
    /// <summary>
    /// Course Offering Doctor ID.
    /// </summary>
    public int CourseOfferingDoctorID { get; set; }

    /// <summary>
    /// Course Offering ID.
    /// </summary>
    public int CourseOfferingID { get; set; }

    /// <summary>
    /// Doctor ID.
    /// </summary>
    public int DoctorID { get; set; }

    /// <summary>
    /// Course Offering.
    /// </summary>
    public CourseOffering CourseOffering { get; set; } = null!;

    /// <summary>
    /// Doctor.
    /// </summary>
    public User Doctor { get; set; } = null!;
}
