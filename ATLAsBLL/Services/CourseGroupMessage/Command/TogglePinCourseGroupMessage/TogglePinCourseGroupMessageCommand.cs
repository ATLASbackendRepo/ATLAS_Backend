using ATLAsBLL.CQRS;

namespace ATLAsBLL.Services.CourseGroupMessage.Command.TogglePinCourseGroupMessage;

public record TogglePinCourseGroupMessageCommand(int MessageId, int RequestingUserId) : ICommand<int>;
