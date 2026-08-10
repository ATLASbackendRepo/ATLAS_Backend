using MediatR;

namespace ATLAsBLL.Features.CourseOfferingTAs.Commands.CreateCourseOfferingTA
{
    public record CreateCourseOfferingTACommand(int CourseOfferingId, int TAID) : ICommand<int>;
}