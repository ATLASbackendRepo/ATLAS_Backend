using MediatR;

namespace ATLAsBLL.Features.CourseOfferings.Commands.DeleteCourseOffering
{
    /// <summary>
    /// Delete Course Offering Command.
    /// </summary>
    public record DeleteCourseOfferingCommand(int CourseOfferingId) : ICommand<bool>, IWriteCommand;
}
