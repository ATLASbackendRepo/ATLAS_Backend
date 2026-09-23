using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ATLASDomain.DTO.CourseGrade;
using MediatR;

namespace ATLAsBLL.Services.CourseGrade.Queries
{

    /// <summary>
    /// Get Course Students Query.
    /// </summary>
    public record GetCourseStudentsQuery(GetStudentsPagedQueryModel QueryModel) : IQuery<PagedResultDto<StudentCardDto>>;
}
