using MediatR;
using System.Collections.Generic;
using ATLASDomain.DTO.CourseOffring;

namespace ATLAsBLL.Services.CourseOffering.Queries.GetmyCourse.Student
{

    /// <summary>
    /// Get My Course Query.
    /// </summary>
    public record GetMyCourseQuery(int UserId) : IQuery<IEnumerable<CourseOfferingDTO>>;
}
