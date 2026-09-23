namespace ATLASDomain.Entities;

/// <summary>
/// Course Offering TA.
/// </summary>
public class CourseOfferingTA
{
    /// <summary>
    /// Course Offering TAID.
    /// </summary>
    public int CourseOfferingTAID { get; set; }

    /// <summary>
    /// Course Offering ID.
    /// </summary>
    public int CourseOfferingID { get; set; }

    /// <summary>
    /// Teacher Assist ID.
    /// </summary>
    public int TeacherAssistID { get; set; }

    /// <summary>
    /// Course Offering.
    /// </summary>
    public CourseOffering CourseOffering { get; set; } = null!;

    /// <summary>
    /// Teaching Assistant.
    /// </summary>
    public User TeachingAssistant { get; set; } = null!;
}
