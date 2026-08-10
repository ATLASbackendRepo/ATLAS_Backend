using MediatR;

namespace ATLAsBLL.Features.CourseOfferingDoctors.Commands.UpdateCourseOfferingDoctor
{
    public record UpdateCourseOfferingDoctorCommand(int CourseOfferingDoctorId, int NewDoctorId) : ICommand<bool>;
}