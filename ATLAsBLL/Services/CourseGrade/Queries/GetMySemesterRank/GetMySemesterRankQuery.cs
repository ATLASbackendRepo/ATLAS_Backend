using ATLASDomain.DTO.CourseGrade;
using MediatR;


namespace ATLAsBLL.Services.CourseGrade.Queries.GetMySemesterRank
{
    public record GetMySemesterRankQuery(int UserId, int SemesterId) : IQuery<MyRankDto?>;

}
