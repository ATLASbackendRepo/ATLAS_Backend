using ATLAsBLL.CQRS;

namespace ATLAsBLL.Services.Notifications.Command.MarkAllNotificationsAsRead;

public record MarkAllNotificationsAsReadCommand(int UserId) : ICommand<int>;
