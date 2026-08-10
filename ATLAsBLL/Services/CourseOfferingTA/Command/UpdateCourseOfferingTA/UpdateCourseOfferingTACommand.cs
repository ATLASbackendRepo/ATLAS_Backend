using MediatR;

namespace ATLAsBLL.Features.CourseOfferingTAs.Commands.UpdateCourseOfferingTA
{
    public record UpdateCourseOfferingTACommand(int CourseOfferingTAId, int NewTAID) : ICommand<bool>;
}