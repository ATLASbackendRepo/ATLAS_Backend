using System.Numerics;
namespace ATLASDomain.Entities;

/// <summary>
/// Department.
/// </summary>
public class Department
{
    /// <summary>
    /// Department Id.
    /// </summary>
    public int DepartmentId { get; set; }

    /// <summary>
    /// D_Name.
    /// </summary>
    public string D_Name { get; set; } = null!;

     /// <summary>
     /// Institution ID.
     /// </summary>
     public int  InstitutionID { get; set; }

    /// <summary>
    /// Institution.
    /// </summary>
    public Institution? Institution { get; set; }

    /// <summary>
    /// Students.
    /// </summary>
    public ICollection<Student> Students { get; set; } = new List<Student>();

    // Users with role Doctor that belong to this department
    /// <summary>
    /// Users.
    /// </summary>
    public ICollection<User> Users { get; set; } = new List<User>();
}
