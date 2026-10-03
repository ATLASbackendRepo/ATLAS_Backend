using ATLASDomain.DTO.Student;
using MediatR;

namespace ATLAsBLL.Features.Students.Commands.UpdateStudent
{
    /// <summary>
    /// Update Student Command.
    /// </summary>
    public record UpdateStudentCommand(
       UpdateStudentDto UpdateStudentDto
    ) : ICommand<bool>, IWriteCommand;
}
