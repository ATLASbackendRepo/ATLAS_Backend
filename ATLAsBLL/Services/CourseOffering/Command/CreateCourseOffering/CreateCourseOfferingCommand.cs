using MediatR;

namespace ATLAsBLL.Features.CourseOfferings.Commands.CreateCourseOffering
{
    public record CreateCourseOfferingCommand(int CourseId, int SemesterId) : ICommand<int>;
}