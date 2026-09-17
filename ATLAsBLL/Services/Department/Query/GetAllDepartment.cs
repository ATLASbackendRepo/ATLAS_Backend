using MediatR;
using ATLASDomain.DTO.Department;

namespace ATLAsBLL.Services.Department.Query
{
    public  record GetAllDepartment(string? Name) : IRequest<List<DepartmentDTO>>
    {
    }
}
