using MediatR;
using ATLAsDCL.UnitOfWork;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.Notifications.Command.MarkNotificationAsRead;

/// <summary>
/// Mark Notification As Read Command Handler.
/// </summary>
public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, int>
{
    /// <summary>
    /// _unit Of Work.
    /// </summary>
    private readonly IUnitOfWork _unitOfWork;

    public MarkNotificationAsReadCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<int> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _unitOfWork.Notifications.GetByIdAsync(request.NotificationId, cancellationToken);

        if (notification == null)
        {
            throw new KeyNotFoundException($"Notification with ID {request.NotificationId} not found.");
        }

        // Verify ownership
        if (notification.RecipientUserID != request.UserId)
        {
            throw new Exceptions.ForbiddenAccessException("You can only mark your own notifications as read.");
        }

        await _unitOfWork.Notifications.MarkAsReadAsync(request.NotificationId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return request.NotificationId;
    }
}
