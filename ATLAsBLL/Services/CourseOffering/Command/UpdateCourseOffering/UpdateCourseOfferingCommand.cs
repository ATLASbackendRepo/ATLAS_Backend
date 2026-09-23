using MediatR;

namespace ATLAsBLL.Features.CourseOfferings.Commands.UpdateCourseOffering
{
    /// <summary>
    /// Update Course Offering Command.
    /// </summary>
    public record UpdateCourseOfferingCommand(int CourseOfferingId, int CourseId, int SemesterId) : ICommand<bool>;
}
