using ATLASDomain.Entities;

namespace ATLAsBLL.Services.CourseGroupMessage.Command.MarkMessageAsRead;
/// <summary>
/// Mark Message As Read Command.
/// </summary>
public record MarkMessageAsReadCommand(int MessageId, int UserId, string Role) : ICommand<int>, IWriteCommand;
