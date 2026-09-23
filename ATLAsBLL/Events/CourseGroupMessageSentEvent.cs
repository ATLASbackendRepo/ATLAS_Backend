using MediatR;

namespace ATLAsBLL.Events
{
    /// <summary>
    /// Course Group Message Sent Event.
    /// </summary>
    public record CourseGroupMessageSentEvent(int MessageId, int CourseOfferingId, int SenderUserId , string Content) : INotification;
}

