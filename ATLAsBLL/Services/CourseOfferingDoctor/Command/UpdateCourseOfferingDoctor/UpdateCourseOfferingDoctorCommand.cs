using MediatR;

namespace ATLAsBLL.Features.CourseOfferingDoctors.Commands.UpdateCourseOfferingDoctor
{
    /// <summary>
    /// Update Course Offering Doctor Command.
    /// </summary>
    public record UpdateCourseOfferingDoctorCommand(int CourseOfferingDoctorId, int NewDoctorId) : ICommand<bool>;
}
