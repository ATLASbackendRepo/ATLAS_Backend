using MediatR;

namespace ATLAsBLL.Features.CourseOfferingTAs.Commands.DeleteCourseOfferingTA
{
    public record DeleteCourseOfferingTACommand(int CourseOfferingTAId) : ICommand<bool>;
}