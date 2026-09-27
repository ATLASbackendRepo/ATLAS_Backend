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
        // The bulk update is executed immediately and reports how many rows it
        // changed, so it is returned instead of the (always zero) number of
        // entities the unit of work saved (LOW-11).
        return await _unitOfWork.Notifications.MarkAllAsReadAsync(request.UserId);
    }
}
