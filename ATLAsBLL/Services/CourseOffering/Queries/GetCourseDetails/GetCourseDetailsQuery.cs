using MediatR;
using System.Collections.Generic;
using ATLASDomain.DTO.CourseOffring;

namespace ATLAsBLL.Services.CourseOffering.Queries.GetCourseDetails
{
    /// <summary>
    /// Get Course Details Query.
    /// </summary>
    public record GetCourseDetailsQuery(int CourseOfferingId, int userId) : IQuery<IEnumerable<CourseofferingDetailsDTO>>;
}
