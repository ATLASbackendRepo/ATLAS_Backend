using ATLAsBLL.Services.Notifications.DTOs;
using ATLAsDCL.UnitOfWork;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.Notifications.Queries.GetMyNotifications;

public class GetMyNotificationsQueryHandler : IRequestHandler<GetMyNotificationsQuery, List<NotificationDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMyNotificationsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<NotificationDto>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<ATLAS.Domain.Entities.Notification> notifications;

        if (request.UnreadOnly)
        {
            notifications = await _unitOfWork.Notifications.GetUnreadForUserAsync(request.UserId);
        }
        else
        {
            notifications = await _unitOfWork.Notifications.FindAsync(n => n.RecipientUserID == request.UserId);
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
