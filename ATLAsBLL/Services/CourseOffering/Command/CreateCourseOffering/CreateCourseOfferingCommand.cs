using MediatR;

namespace ATLAsBLL.Features.CourseOfferings.Commands.CreateCourseOffering
{
    /// <summary>
    /// Create Course Offering Command.
    /// </summary>
    public record CreateCourseOfferingCommand(int CourseId, int SemesterId) : ICommand<int>;
}
