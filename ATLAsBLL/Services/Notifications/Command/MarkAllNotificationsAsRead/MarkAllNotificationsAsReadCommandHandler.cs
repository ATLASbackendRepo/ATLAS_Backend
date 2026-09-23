using MediatR;
using ATLAsDCL.UnitOfWork;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.Notifications.Command.MarkAllNotificationsAsRead;

/// <summary>
/// Mark All Notifications As Read Command Handler.
/// </summary>
public class MarkAllNotificationsAsReadCommandHandler : IRequestHandler<MarkAllNotificationsAsReadCommand, int>
{
    /// <summary>
    /// _unit Of Work.
    /// </summary>
    private readonly IUnitOfWork _unitOfWork;

    public MarkAllNotificationsAsReadCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<int> Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.Notifications.MarkAllAsReadAsync(request.UserId);
        var count = await _unitOfWork.SaveChangesAsync(cancellationToken);

        return count;
    }
}
