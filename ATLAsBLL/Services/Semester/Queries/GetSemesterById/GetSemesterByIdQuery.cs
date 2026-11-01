using ATLASDomain.DTO.Semester;
using MediatR;

namespace ATLAsBLL.Features.Semesters.Queries.GetSemesterById
{
    /// <summary>
    /// Get Semester By Id Query.
    /// </summary>
    public record GetSemesterByIdQuery(int Id) : IRequest<SemesterDto>;
}

