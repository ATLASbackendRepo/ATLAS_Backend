using ATLAsBLL.CQRS;
using ATLASDomain.Entities;

namespace ATLAsBLL.Services.CourseGroupMessage.Command.MarkMessageAsRead;

public record MarkMessageAsReadCommand(int MessageId, int UserId, string Role) : ICommand<int>;
