using ATLASDomain.DTO.Department;
using ATLASDomain.Entities;
using ATLASDomain.InterFaces.Repositories;

namespace ATLASDomain.Interfaces.Repositories;

public interface IDepartmentRepository : IGenericRepository<Department>
{
    Task<Department?> GetDepartmentWithStudentsAsync(int departmentId);

    Task<Department?> GetDepartmentWithDoctorsAsync(int departmentId);

    Task<int> GetDepartmentWithnameAsync(string departmentName, int institutionId);

    Task<List<DepartmentDTO>> GetDepartments(string? departmentName);
}