using MediatR;

namespace ATLAsBLL.Features.CourseOfferingDoctors.Commands.DeleteCourseOfferingDoctor
{
    public record DeleteCourseOfferingDoctorCommand(int CourseOfferingDoctorId) : ICommand<bool>;
}