using ATLASDomain.DTO.Course;
using MediatR;


namespace ATLAsBLL.Services.Course.Queries
{
    /// <summary>
    /// Get All Course Query.
    /// </summary>
    public  record GetAllCourseQuery(string? courseName) : IRequest<List<CourseDTO>>
    {
    }
}
