using MediatR;

namespace ATLAsBLL.Features.Department.Comand.DeleteDepartment
{
    /// <summary>
    /// Delete Department Command.
    /// </summary>
    public record DeleteDepartmentCommand(int DepartmentID, int? InstitutionId = null) : ICommand<bool>, IWriteCommand;
}
