using MediatR;


namespace ATLAsBLL.Services.CourseOfferingDoctor.Query
{
    /// <summary>
    /// Get Doctor IDQuery.
    /// </summary>
    public record GetDoctorIDQuery(int CourseOfferingID) : IRequest<IEnumerable<int>>;

}
