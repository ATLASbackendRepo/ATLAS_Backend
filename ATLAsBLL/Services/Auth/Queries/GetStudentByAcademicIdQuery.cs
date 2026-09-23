using MediatR;

namespace ATLAsBLL.Services.Auth.Queries
{
    /// <summary>
    /// Get User By Academic Id Query.
    /// </summary>
    public sealed record GetUserByAcademicIdQuery(string AcademicId)
      : IQuery<ATLASDomain.Entities.User?>;
}
