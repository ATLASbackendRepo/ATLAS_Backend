using MediatR;

namespace ATLAsBLL.Features.Department.Comand.CreateDepartment
{
    public record CreateDepartmentCommand(string DepartmentName, int InstitutionId) : ICommand<int>;
}