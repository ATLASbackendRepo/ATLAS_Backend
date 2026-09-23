
    using MediatR;


namespace ATLAsBLL.Services.User.qeusies
{

    /// <summary>
    /// Get User By Id Query.
    /// </summary>
    public sealed record GetUserByIdQuery(int UserId) : IQuery<ATLASDomain.Entities.User?>;
}
