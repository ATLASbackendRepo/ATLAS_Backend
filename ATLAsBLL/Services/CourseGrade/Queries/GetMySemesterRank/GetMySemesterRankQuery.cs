using ATLASDomain.DTO.CourseGrade;
using MediatR;


namespace ATLAsBLL.Services.CourseGrade.Queries.GetMySemesterRank
{
    /// <summary>
    /// Get My Semester Rank Query.
    /// </summary>
    public record GetMySemesterRankQuery(int UserId, int SemesterId) : IQuery<MyRankDto?>;

}
