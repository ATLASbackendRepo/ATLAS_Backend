using ATLASDomain.DTO.Student;
using MediatR;

namespace ATLAsBLL.Features.Students.Commands.CreateStudent
{
    /// <summary>
    /// Create Student Command.
    /// </summary>
    public record CreateStudentCommand(
       CreateStudentDTO CreateStudentDTO
    ) : ICommand<int>; 
}
