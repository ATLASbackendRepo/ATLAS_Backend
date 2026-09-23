using MediatR;

namespace ATLAsBLL.Features.CourseOfferingTAs.Commands.UpdateCourseOfferingTA
{
    /// <summary>
    /// Update Course Offering TACommand.
    /// </summary>
    public record UpdateCourseOfferingTACommand(int CourseOfferingTAId, int NewTAID) : ICommand<bool>;
}
