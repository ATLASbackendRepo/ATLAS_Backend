using MediatR;

namespace ATLAsBLL.Features.Students.Commands.DeleteStudent
{
    public record DeleteStudentCommand(int StudentId) : ICommand<bool>;
}