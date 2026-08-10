using MediatR;

namespace ATLAsBLL.Features.Enrollments.Commands.CreateEnrollment
{
    public record CreateEnrollmentCommand(int StudentId, int CourseOfferingId) : ICommand<int>;
}