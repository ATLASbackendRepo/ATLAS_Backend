using MediatR;

namespace ATLAsBLL.Features.Department.Comand.UpdateDepartment
{
    public record UpdateDepartmentCommand(int DepartmentID, string DepartmentName) : ICommand<bool>;
}