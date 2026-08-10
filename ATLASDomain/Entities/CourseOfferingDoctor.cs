namespace ATLASDomain.Entities;

public class CourseOfferingDoctor
{
    public int CourseOfferingDoctorID { get; set; }

    public int CourseOfferingID { get; set; }

    public int DoctorID { get; set; }

    public CourseOffering CourseOffering { get; set; } = null!;

    public User Doctor { get; set; } = null!;
}