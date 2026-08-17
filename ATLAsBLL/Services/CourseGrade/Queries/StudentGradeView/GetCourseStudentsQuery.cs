using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ATLASDomain.DTO.CourseGrade;
using MediatR;

namespace ATLAsBLL.Services.CourseGrade.Queries
{

    public record GetCourseStudentsQuery(GetStudentsPagedQueryModel QueryModel) : IQuery<PagedResultDto<StudentCardDto>>;
}
