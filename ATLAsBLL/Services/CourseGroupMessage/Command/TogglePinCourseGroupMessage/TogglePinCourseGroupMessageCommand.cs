using ATLAsBLL.CQRS;

namespace ATLAsBLL.Services.CourseGroupMessage.Command.TogglePinCourseGroupMessage;

/// <summary>
/// Toggle Pin Course Group Message Command.
/// </summary>
public record TogglePinCourseGroupMessageCommand(int MessageId, int RequestingUserId) : ICommand<int>;
