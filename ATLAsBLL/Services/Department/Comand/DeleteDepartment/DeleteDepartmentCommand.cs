using MediatR;

namespace ATLAsBLL.Features.Department.Comand.DeleteDepartment
{
    public record DeleteDepartmentCommand(int DepartmentID) : ICommand<bool>;
}