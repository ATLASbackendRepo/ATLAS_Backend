using ATLAsDCL.UnitOfWork;
using ATLASDomain.DTO.Department;
using MediatR;

namespace ATLAsBLL.Services.Department.Query;

public class GetAllDepartmentHandler : IRequestHandler<GetAllDepartment, List<DepartmentDTO>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllDepartmentHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<DepartmentDTO>> Handle(GetAllDepartment request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Departments.GetDepartments(request.Name);
    }
}