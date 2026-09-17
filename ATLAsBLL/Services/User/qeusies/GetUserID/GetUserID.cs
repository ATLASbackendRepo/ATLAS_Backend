using MediatR;
namespace ATLAsBLL.Services.User.qeusies.GetUserID
{
    public record GetUserID(string AcdimicID) : IRequest<int?>;
}
