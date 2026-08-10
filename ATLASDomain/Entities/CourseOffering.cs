namespace ATLASDomain.Entities;

public class CourseOffering
{
    public int CourseOfferingID { get; set; }

    public int SemesterID { get; set; }

    public int CourseID { get; set; }

    public Semester Semester { get; set; } = null!;

    public Course Course { get; set; } = null!;

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public ICollection<CourseOfferingDoctor> CourseOfferingDoctors { get; set; } = new List<CourseOfferingDoctor>();

    public ICollection<CourseOfferingTA> CourseOfferingTAs { get; set; } = new List<CourseOfferingTA>();
    public ICollection<AssessmentModule> AssessmentModules { get; set; } = new List<AssessmentModule>();
}