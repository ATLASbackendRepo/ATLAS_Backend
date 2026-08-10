
    using MediatR;


namespace ATLAsBLL.Services.User.qeusies
{

    public sealed record GetUserByIdQuery(int UserId) : IQuery<ATLASDomain.Entities.User?>;
}
