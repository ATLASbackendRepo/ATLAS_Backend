using ATLASDomain.DTO.Semester;
using MediatR;
using System.Collections.Generic;

namespace ATLAsBLL.Features.Semesters.Queries.GetSemesters
{
    /// <summary>
    /// Get Semesters Query.
    /// </summary>
    public record GetSemestersQuery : IRequest<List<SemesterDto>>;
}

