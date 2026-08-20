using MediatR;

namespace ATLAsBLL.Events
{
    public record CourseGroupMessageSentEvent(int MessageId, int CourseOfferingId, int SenderUserId , string Content) : INotification;
}

