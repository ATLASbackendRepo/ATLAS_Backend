using MediatR;

namespace ATLAsBLL.Features.Department.Comand.UpdateDepartment
{
    /// <summary>
    /// Update Department Command.
    /// </summary>
    public record UpdateDepartmentCommand(int DepartmentID, string DepartmentName, int? InstitutionId = null) : ICommand<bool>, IWriteCommand;
}
