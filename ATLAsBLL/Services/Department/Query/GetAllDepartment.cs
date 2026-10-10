using MediatR;
using ATLASDomain.DTO.Department;

namespace ATLAsBLL.Services.Department.Query
{
    /// <summary>
    /// Get All Department.
    /// </summary>
    public record GetAllDepartment(string? Name, int? InstitutionId = null) : IRequest<List<DepartmentDTO>>
    {
    }
}
