namespace ATLAsBLL.Services.Notifications.Command.MarkNotificationAsRead;
/// <summary>
/// Mark Notification As Read Command.
/// </summary>
public record MarkNotificationAsReadCommand(int NotificationId, int UserId) : ICommand<int>, IWriteCommand;
