namespace ATLASDomain.Entities;

public class CourseOfferingTA
{
    public int CourseOfferingTAID { get; set; }

    public int CourseOfferingID { get; set; }

    public int TAID { get; set; }

    public CourseOffering CourseOffering { get; set; } = null!;

    public User TeachingAssistant { get; set; } = null!;
}