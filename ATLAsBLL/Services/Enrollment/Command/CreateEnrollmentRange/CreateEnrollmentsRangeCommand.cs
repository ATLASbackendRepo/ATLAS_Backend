using MediatR;
using System.Collections.Generic;

namespace ATLAsBLL.Features.Enrollments.Commands.CreateEnrollmentsRange
{
    public record CreateEnrollmentsRangeCommand(List<EnrollmentItemDto> Enrollments) : ICommand<bool>;

    public record EnrollmentItemDto(int StudentId, int CourseOfferingId);
}