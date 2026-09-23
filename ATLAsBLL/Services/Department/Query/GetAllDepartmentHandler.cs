using ATLAsDCL.UnitOfWork;
using ATLASDomain.DTO.Department;
using MediatR;

namespace ATLAsBLL.Services.Department.Query;

/// <summary>
/// Get All Department Handler.
/// </summary>
public class GetAllDepartmentHandler : IRequestHandler<GetAllDepartment, List<DepartmentDTO>>
{
    /// <summary>
    /// _unit Of Work.
    /// </summary>
    private readonly IUnitOfWork _unitOfWork;

    public GetAllDepartmentHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<List<DepartmentDTO>> Handle(GetAllDepartment request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Departments.GetDepartments(request.Name);
    }
}
