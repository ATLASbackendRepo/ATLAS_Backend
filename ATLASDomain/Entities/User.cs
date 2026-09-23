using System.Numerics;
namespace ATLASDomain.Entities;
    /// <summary>
    /// en User Role.
    /// </summary>
    public enum enUserRole
    {
        Student = 1,
        Doctor = 2,
        TeachingAssistant = 3,
        Admin = 4,
        SAdmin = 5
    }

/// <summary>
/// User.
/// </summary>
public class User
{
    /// <summary>
    /// User Id.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Full Name.
    /// </summary>
    public string FullName { get; set; } = null!;

    /// <summary>
    /// Password.
    /// </summary>
    public string Password { get; set; } = null!;


    /// <summary>
    /// Is Active.
    /// </summary>
    public bool IsActive { get; set; }
    /// <summary>
    /// Must Change Password.
    /// </summary>
    public bool MustChangePassword { get; set; }

    /// <summary>
    /// Created At.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// Role.
    /// </summary>
    public enUserRole Role { get; set; }

    /// <summary>
    /// Academic Id.
    /// </summary>
    public required string AcademicId { get; set; }
    /// <summary>
    /// Institution ID.
    /// </summary>
    public int InstitutionID { get; set; }
    /// <summary>
    /// Department Id.
    /// </summary>
    public int DepartmentId { get; set; }

   
    /// <summary>
    /// Institution.
    /// </summary>
    public Institution? Institution { get; set; }

    /// <summary>
    /// Department.
    /// </summary>
    public Department? Department { get; set; }



    /// <summary>
    /// Student.
    /// </summary>
    public Student? Student { get; set; }

    //if user ia a teaching assistant, then he can have many course offerings
    /// <summary>
    /// Course Offering TAs.
    /// </summary>
    public ICollection<CourseOfferingTA>? CourseOfferingTAs { get; set; } = new List<CourseOfferingTA>();

    // if user is a doctor, then he can have many course offerings
    /// <summary>
    /// Course Offering Doctors.
    /// </summary>
    public ICollection<CourseOfferingDoctor>? CourseOfferingDoctors { get; set; } = new List<CourseOfferingDoctor>();

    // if user is a super admin, then he can have many enrollments
    /// <summary>
    /// Enrollments.
    /// </summary>
    public ICollection<Enrollment>? Enrollments { get; set; } = new List<Enrollment>();





    /// <summary>
    ///  authentication
    /// </summary>

    public string? RefreshTokenHash { get; set; }
    /// <summary>
    /// Previous Refresh Token Hash.
    /// </summary>
    public string? PreviousRefreshTokenHash { get; set; }
    /// <summary>
    /// Refresh Token Expires At.
    /// </summary>
    public DateTime? RefreshTokenExpiresAt { get; set; }
    /// <summary>
    /// Refresh Token Revoked At.
    /// </summary>
    public DateTime? RefreshTokenRevokedAt { get; set; }
}
