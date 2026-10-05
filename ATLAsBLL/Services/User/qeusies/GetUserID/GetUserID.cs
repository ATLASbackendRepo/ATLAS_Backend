using MediatR;

namespace ATLAsBLL.Services.User.qeusies.GetUserID
{
    /// <summary>
    /// Get User ID.
    /// </summary>
    public record GetUserID(string AcdimicID) : IRequest<int?>;
}
