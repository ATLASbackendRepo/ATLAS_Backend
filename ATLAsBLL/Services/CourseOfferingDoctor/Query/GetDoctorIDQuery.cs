using MediatR;


namespace ATLAsBLL.Services.CourseOfferingDoctor.Query
{
    public record GetDoctorIDQuery(int CourseOfferingID) : IRequest<IEnumerable<int>>;

}
