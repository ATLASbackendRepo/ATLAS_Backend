using MediatR;

namespace ATLAsBLL.Features.CourseOfferings.Commands.DeleteCourseOffering
{
    public record DeleteCourseOfferingCommand(int CourseOfferingId) : ICommand<bool>;
}