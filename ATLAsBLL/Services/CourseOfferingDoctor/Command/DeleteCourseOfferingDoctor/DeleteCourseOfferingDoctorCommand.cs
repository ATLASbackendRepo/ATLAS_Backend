using MediatR;

namespace ATLAsBLL.Features.CourseOfferingDoctors.Commands.DeleteCourseOfferingDoctor
{
    /// <summary>
    /// Delete Course Offering Doctor Command.
    /// </summary>
    public record DeleteCourseOfferingDoctorCommand(int CourseOfferingDoctorId) : ICommand<bool>;
}
