using System.Numerics;
namespace ATLASDomain.Entities;

public class Department
{
    public int DepartmentId { get; set; }

    public string D_Name { get; set; } = null!;

     public int  InstitutionID { get; set; }

    public Institution? Institution { get; set; }

    public ICollection<Student> Students { get; set; } = new List<Student>();

    // Users with role Doctor that belong to this department
    public ICollection<User> Users { get; set; } = new List<User>();
}