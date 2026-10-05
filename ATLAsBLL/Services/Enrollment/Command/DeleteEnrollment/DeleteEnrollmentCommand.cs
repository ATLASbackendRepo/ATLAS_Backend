using MediatR;

namespace ATLAsBLL.Features.Enrollments.Commands.DeleteEnrollment
{
    /// <summary>
    /// Delete Enrollment Command.
    /// </summary>
    public record DeleteEnrollmentCommand(int EnrollmentID) : ICommand<bool>, IWriteCommand;
}
