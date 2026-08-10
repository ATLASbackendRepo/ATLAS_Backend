using System.Numerics;
namespace ATLASDomain.Entities;
    public enum enUserRole
    {
        Student = 1,
        Doctor = 2,
        TeachingAssistant = 3,
        Admin = 4,
        SAdmin = 5
    }

public class User
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public enUserRole Role { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    public Student? Student { get; set; }

    //if user ia a teaching assistant, then he can have many course offerings
    public ICollection<CourseOfferingTA>? CourseOfferingTAs { get; set; } = new List<CourseOfferingTA>();

    // if user is a doctor, then he can have many course offerings
    public ICollection<CourseOfferingDoctor>? CourseOfferingDoctors { get; set; } = new List<CourseOfferingDoctor>();

    // if user is a super admin, then he can have many enrollments
    public ICollection<Enrollment>? Enrollments { get; set; } = new List<Enrollment>();
}