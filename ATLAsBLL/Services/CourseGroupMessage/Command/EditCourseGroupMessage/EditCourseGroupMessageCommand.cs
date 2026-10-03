namespace ATLAsBLL.Services.CourseGroupMessage.Command.EditCourseGroupMessage;
/// <summary>
/// Edit Course Group Message Command.
/// </summary>
public record EditCourseGroupMessageCommand(int MessageId, int RequestingUserId, string NewContent) : ICommand<int>, IWriteCommand;
