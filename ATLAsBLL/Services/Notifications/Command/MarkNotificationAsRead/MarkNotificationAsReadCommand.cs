using ATLAsBLL.CQRS;

namespace ATLAsBLL.Services.Notifications.Command.MarkNotificationAsRead;

public record MarkNotificationAsReadCommand(int NotificationId, int UserId) : ICommand<int>;
