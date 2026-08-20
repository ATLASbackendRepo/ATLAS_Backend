using MediatR;
using System.Collections.Generic;
using ATLASDomain.DTO.CourseOffring;

namespace ATLAsBLL.Services.CourseOffering.Queries.GetCourseDetails
{
    public record GetCourseDetailsQuery(int CourseOfferingId, int userId) : IQuery<IEnumerable<CourseofferingDetailsDTO>>;
}
