using MediatR;

namespace ATLAsBLL.Services.Auth.Queries
{
    public sealed record GetUserByAcademicIdQuery(string AcademicId)
      : IQuery<ATLASDomain.Entities.User?>;
}
