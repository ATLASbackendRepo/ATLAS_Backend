using ATLAsBLL.CQRS;

namespace ATLAsBLL.Services.CourseGroupMessage.Command.MarkMessageAsRead;

public record MarkMessageAsReadCommand(int MessageId, int StudentId) : ICommand<int>;
