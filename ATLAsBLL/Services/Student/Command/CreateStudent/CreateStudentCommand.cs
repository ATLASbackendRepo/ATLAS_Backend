using ATLASDomain.DTO.Student;
using MediatR;

namespace ATLAsBLL.Features.Students.Commands.CreateStudent
{
    public record CreateStudentCommand(
       CreateStudentDTO CreateStudentDTO
    ) : ICommand<int>; 
}
