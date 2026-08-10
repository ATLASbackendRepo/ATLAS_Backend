namespace ATLASDomain.Entities;

public class Enrollment
{
    public int EnrollmentID { get; set; }

    public int StudentID { get; set; }

    public int CreatedBySuperAdminID { get; set; }
    public DateTime EnrollmentDate { get; set; }

    public int CourseOfferingID { get; set; }

    public Student Student { get; set; } = null!;

    public User CreatedBySuperAdmin { get; set; } = null!;

    public CourseOffering CourseOffering { get; set; } = null!;
    //public CourseGrade?  CourseGrade { get; set; } = null!;

    public ICollection<CourseGrade> CourseGrades { get; set; } = new List<CourseGrade>();

}