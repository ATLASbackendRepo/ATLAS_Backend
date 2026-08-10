using MediatR;

namespace ATLAsBLL.Features.CourseOfferingDoctors.Commands.CreateCourseOfferingDoctor
{
    public record CreateCourseOfferingDoctorCommand(int CourseOfferingId, int DoctorId) : ICommand<int>;
}