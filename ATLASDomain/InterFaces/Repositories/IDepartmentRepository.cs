using ATLASDomain.DTO.Department;
using ATLASDomain.Entities;
using ATLASDomain.InterFaces.Repositories;

namespace ATLASDomain.Interfaces.Repositories;

/// <summary>
/// IDepartment Repository.
/// </summary>
public interface IDepartmentRepository : IGenericRepository<Department>
{
    /// <summary>
    /// Gets a department together with its students.
    /// </summary>
    /// <param name="departmentId">Department identifier.</param>
    /// <returns>The department, or null when it does not exist.</returns>
    Task<Department?> GetDepartmentWithStudentsAsync(int departmentId);

    /// <summary>
    /// Gets a department together with its doctors.
    /// </summary>
    /// <param name="departmentId">Department identifier.</param>
    /// <returns>The department, or null when it does not exist.</returns>
    Task<Department?> GetDepartmentWithDoctorsAsync(int departmentId);

    /// <summary>
    /// Looks up a department by name within an institution.
    /// </summary>
    /// <param name="departmentName">Department name to match.</param>
    /// <param name="institutionId">Institution that owns the department.</param>
    /// <returns>The department id, or 0 when no such department exists.</returns>
    Task<int> GetDepartmentWithnameAsync(string departmentName, int institutionId);

    /// <summary>
    /// Gets the departments, optionally filtered by a name fragment.
    /// </summary>
    /// <param name="departmentName">
    /// Substring to match against the department name; null returns everything.
    /// </param>
    /// <returns>The matching departments.</returns>
    Task<List<DepartmentDTO>> GetDepartments(string? departmentName);
}
