using MediatR;

namespace ATLAsBLL.Features.CourseOfferingTAs.Commands.DeleteCourseOfferingTA
{
    /// <summary>
    /// Delete Course Offering TACommand.
    /// </summary>
    public record DeleteCourseOfferingTACommand(int CourseOfferingTAId) : ICommand<bool>, IWriteCommand;
}
