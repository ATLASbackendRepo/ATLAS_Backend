using ATLAsBLL.CQRS;

namespace ATLAsBLL.Services.CourseGroupMessage.Command.DeleteCourseGroupMessage;

/// <summary>
/// Delete Course Group Message Command.
/// </summary>
public record DeleteCourseGroupMessageCommand(int MessageId, int RequestingUserId) : ICommand<int>;
