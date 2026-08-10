using MediatR;

namespace ATLAsBLL.Features.Department.Comand.CreateDepartment
{
    public record CreateDepartmentCommand(string DepartmentName) : ICommand<int>;
}