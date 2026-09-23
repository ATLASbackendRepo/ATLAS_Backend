using MediatR;

namespace ATLAsBLL.Features.Students.Commands.DeleteStudent
{
    /// <summary>
    /// Delete Student Command.
    /// </summary>
    public record DeleteStudentCommand(string StudentCode) : ICommand<bool>;
}
