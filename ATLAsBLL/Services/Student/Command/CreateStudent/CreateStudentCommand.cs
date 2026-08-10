using MediatR;

namespace ATLAsBLL.Features.Students.Commands.CreateStudent
{
    public record CreateStudentCommand(
        string FullName,
        string Password,
        string StudentCode,
        string AcademicId,
        int AcademicLevel,
        int DepartmentId
    ) : ICommand<int>; 
}
