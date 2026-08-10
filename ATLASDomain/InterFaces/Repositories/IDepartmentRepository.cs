using ATLASDomain.Entities;
using ATLASDomain.InterFaces.Repositories;

namespace ATLASDomain.Interfaces.Repositories;

public interface IDepartmentRepository : IGenericRepository<Department>
{
    Task<Department?> GetDepartmentWithStudentsAsync(int departmentId);

    Task<Department?> GetDepartmentWithDoctorsAsync(int departmentId);
}