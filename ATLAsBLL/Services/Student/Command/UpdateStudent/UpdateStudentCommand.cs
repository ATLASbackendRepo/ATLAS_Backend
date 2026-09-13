using ATLASDomain.DTO.Student;
using MediatR;

namespace ATLAsBLL.Features.Students.Commands.UpdateStudent
{
    public record UpdateStudentCommand(

       UpdateStudentDto UpdateStudentDto
    ) : ICommand<bool>;
}