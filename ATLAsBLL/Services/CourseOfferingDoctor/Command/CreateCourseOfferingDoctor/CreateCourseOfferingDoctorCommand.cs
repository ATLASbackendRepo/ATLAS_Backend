using MediatR;

namespace ATLAsBLL.Features.CourseOfferingDoctors.Commands.CreateCourseOfferingDoctor
{
    /// <summary>
    /// Create Course Offering Doctor Command.
    /// </summary>
    public record CreateCourseOfferingDoctorCommand(int CourseOfferingId, int DoctorId) : ICommand<int>;
}
