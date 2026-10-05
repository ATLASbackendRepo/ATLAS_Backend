using ATLAsBLL.Services.Notifications.DTOs;
using ATLAsDCL.UnitOfWork;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.Notifications.Queries.GetMyNotifications;

/// <summary>
/// Get My Notifications Query Handler.
/// </summary>
public class GetMyNotificationsQueryHandler : IRequestHandler<GetMyNotificationsQuery, List<NotificationDto>>
{
    /// <summary>
    /// _unit Of Work.
    /// </summary>
    private readonly IUnitOfWork _unitOfWork;

    public GetMyNotificationsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<List<NotificationDto>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<ATLAS.Domain.Entities.Notification> notifications;

        if (request.UnreadOnly)
        {
            notifications = await _unitOfWork.Notifications.GetUnreadForUserAsync(request.UserId, cancellationToken);
        }
        else
        {
            notifications = await _unitOfWork.Notifications.FindAsync(n => n.RecipientUserID == request.UserId, cancellationToken);
        }

        // Map to anonymous objects for API response
        var result = notifications.Select(n => new NotificationDto
        {
            NotificationID = n.NotificationID,
            RecipientUserID = n.RecipientUserID,
            Type = n.Type,
            Title = n.Title,
            Body = n.Body,
            RelatedMessageID = n.RelatedMessageID,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        }).ToList();

        return result;
    }
}
