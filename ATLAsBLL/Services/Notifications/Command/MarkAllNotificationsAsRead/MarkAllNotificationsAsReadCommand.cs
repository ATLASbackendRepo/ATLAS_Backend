using ATLAsBLL.CQRS;

namespace ATLAsBLL.Services.Notifications.Command.MarkAllNotificationsAsRead;

/// <summary>
/// Mark All Notifications As Read Command.
/// </summary>
public record MarkAllNotificationsAsReadCommand(int UserId) : ICommand<int>;
