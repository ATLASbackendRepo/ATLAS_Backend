using MediatR;
using System.Collections.Generic;
using ATLASDomain.DTO.CourseOffring;

namespace ATLAsBLL.Services.CourseOffering.Queries.GetmyCourse.Student
{

    public record GetMyCourseQuery(int StudentId) : IQuery<IEnumerable<CourseOfferingDTO>>;
}