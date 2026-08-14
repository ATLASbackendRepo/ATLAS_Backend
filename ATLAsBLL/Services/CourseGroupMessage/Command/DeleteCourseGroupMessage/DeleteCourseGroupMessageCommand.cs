using ATLAsBLL.CQRS;

namespace ATLAsBLL.Services.CourseGroupMessage.Command.DeleteCourseGroupMessage;

public record DeleteCourseGroupMessageCommand(int MessageId, int RequestingUserId) : ICommand<int>;
