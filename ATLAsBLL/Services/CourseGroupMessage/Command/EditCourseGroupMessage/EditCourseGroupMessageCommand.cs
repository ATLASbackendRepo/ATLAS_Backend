using ATLAsBLL.CQRS;

namespace ATLAsBLL.Services.CourseGroupMessage.Command.EditCourseGroupMessage;

public record EditCourseGroupMessageCommand(int MessageId, int RequestingUserId, string NewContent) : ICommand<int>;
