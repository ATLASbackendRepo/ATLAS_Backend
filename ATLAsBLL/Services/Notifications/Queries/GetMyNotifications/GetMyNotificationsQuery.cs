using ATLAsBLL.CQRS;
using ATLAsBLL.Services.Notifications.DTOs;

namespace ATLAsBLL.Services.Notifications.Queries.GetMyNotifications;

/// <summary>
/// Get My Notifications Query.
/// </summary>
public record GetMyNotificationsQuery(int UserId, bool UnreadOnly = false) : IQuery<List<NotificationDto>>;
