using ATLAsBLL.CQRS;

namespace ATLAsBLL.Services.CourseGroupMessage.Command.SendCourseGroupMessage
{
    public record SendCourseGroupMessageCommand(int CourseOfferingId, int SenderUserId, string Content) : ICommand<int>;
}
