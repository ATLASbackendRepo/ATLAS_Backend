using ATLASDomain.DTO.CourseGrade;
using MediatR;


namespace ATLAsBLL.Services.CourseGrade.Queries.GetMySemesterRank
{
    public record GetMySemesterRankQuery(int StudentId, int SemesterId) : IQuery<MyRankDto?>;

}
