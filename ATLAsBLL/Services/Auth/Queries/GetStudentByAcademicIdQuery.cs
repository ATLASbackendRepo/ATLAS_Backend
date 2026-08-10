using MediatR;

namespace ATLAsBLL.Services.Auth.Queries
{
    public sealed record GetStudentByAcademicIdQuery(string AcademicId)
      : IQuery<ATLASDomain.Entities.Student?>;
}
