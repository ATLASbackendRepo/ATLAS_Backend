using MediatR;

namespace ATLAsBLL.Features.CourseOfferings.Commands.UpdateCourseOffering
{
    public record UpdateCourseOfferingCommand(int CourseOfferingId, int CourseId, int SemesterId) : ICommand<bool>;
}