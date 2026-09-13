using MediatR;

namespace ATLAsBLL.Features.Students.Commands.DeleteStudent
{
    public record DeleteStudentCommand(string StudentCode) : ICommand<bool>;
}