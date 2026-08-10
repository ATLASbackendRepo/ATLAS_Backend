using MediatR;

namespace ATLAsBLL.Features.Enrollments.Commands.DeleteEnrollment
{
    public record DeleteEnrollmentCommand(int EnrollmentID) : ICommand<bool>;
}