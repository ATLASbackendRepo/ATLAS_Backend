using MediatR;

namespace ATLAsBLL.Features.Department.Comand.CreateDepartment
{
    /// <summary>
    /// Create Department Command.
    /// </summary>
    public record CreateDepartmentCommand(string DepartmentName, int InstitutionId) : ICommand<int>, IWriteCommand;
}
