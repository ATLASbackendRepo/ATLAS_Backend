namespace ATLASDomain.Entities;

public class Student
{
    public int StudentId { get; set; }
    public string StudentCode { get; set; } = null!;
    public int UserId { get; set; }
    public int AcademicLevel { get; set; }
    public bool IsActive { get; set; }
    public string? AcademicId { get; set; } 
    public int InstitutionID { get; set; }
    public int DepartmentId { get; set; }
     public  User? User { get; set; }  
    public Department? Department { get; set; } = null!;
    public Institution? Institution { get; set; } = null!;

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
