using ATLASDomain.DTO.Course;
using MediatR;


namespace ATLAsBLL.Services.Course.Queries
{
    public  record GetAllCourseQuery(string? courseName) : IRequest<List<CourseDTO>>
    {
    }
}
