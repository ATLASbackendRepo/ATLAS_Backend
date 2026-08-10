using MediatR;

namespace ATLAsBLL.Features.Students.Commands.UpdateStudent
{
    public record UpdateStudentCommand(
        int StudentId,
        string FullName,
      
        string StudentCode,
        int AcademicLevel,
        int DepartmentId
    ) : ICommand<bool>;
}