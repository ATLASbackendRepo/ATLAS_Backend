namespace ATLAsBLL.Services.CourseGroupMessage.Command.SendCourseGroupMessage
{
    /// <summary>
    /// Send Course Group Message Command.
    /// </summary>
    public record SendCourseGroupMessageCommand(int CourseOfferingId, int SenderUserId, string Content ) : ICommand<int>, IWriteCommand;
}
