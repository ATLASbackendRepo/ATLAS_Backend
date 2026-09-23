using MediatR;

namespace ATLAsBLL.Features.CourseOfferingTAs.Commands.CreateCourseOfferingTA
{
    /// <summary>
    /// Create Course Offering TACommand.
    /// </summary>
    public record CreateCourseOfferingTACommand(int CourseOfferingId, int TAID) : ICommand<int>;
}
