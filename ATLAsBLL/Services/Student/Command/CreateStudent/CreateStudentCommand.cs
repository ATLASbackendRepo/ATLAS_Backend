using ATLASDomain.DTO.Student;
using ATLAsBLL.Services.Common;
using MediatR;

namespace ATLAsBLL.Features.Students.Commands.CreateStudent
{
    /// <summary>
    /// Create Student Command.
    /// </summary>
    public record CreateStudentCommand(
       CreateStudentDTO CreateStudentDTO
    ) : ICommand<CreateStudentResult>, IWriteCommand; 
}