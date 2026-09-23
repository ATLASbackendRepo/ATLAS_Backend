namespace ATLASDomain.Entities;

/// <summary>
/// Student.
/// </summary>
public class Student
{
    /// <summary>
    /// Student Id.
    /// </summary>
    public int StudentId { get; set; }
    /// <summary>
    /// Student Code.
    /// </summary>
    public string StudentCode { get; set; } = null!;
    /// <summary>
    /// User Id.
    /// </summary>
    public int? UserId { get; set; }
    /// <summary>
    /// Academic Level.
    /// </summary>
    public int AcademicLevel { get; set; }
    /// <summary>
    /// Is Active.
    /// </summary>
    public bool IsActive { get; set; }
    /// <summary>
    /// Academic Id.
    /// </summary>
    public string? AcademicId { get; set; } 
    /// <summary>
    /// Institution ID.
    /// </summary>
    public int InstitutionID { get; set; }
    /// <summary>
    /// Department Id.
    /// </summary>
    public int DepartmentId { get; set; }
     /// <summary>
     /// User.
     /// </summary>
     public  User? User { get; set; }  
    /// <summary>
    /// Department.
    /// </summary>
    public Department? Department { get; set; } = null!;
    /// <summary>
    /// Institution.
    /// </summary>
    public Institution? Institution { get; set; } = null!;

    /// <summary>
    /// Enrollments.
    /// </summary>
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
