using MediatR;
using ATLAsBLL.Exceptions;
using ATLAsBLL.Events;
using ATLASDomain.Entities;
using ATLAsDCL.UnitOfWork;
using ATLASDomain.Services;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.CourseGroupMessage.Command.SendCourseGroupMessage;

/// <summary>
/// Send Course Group Message Command Handler.
/// </summary>
public class SendCourseGroupMessageCommandHandler : IRequestHandler<SendCourseGroupMessageCommand, int>
{
    /// <summary>
    /// _unit Of Work.
    /// </summary>
    private readonly IUnitOfWork _unitOfWork;
    /// <summary>
    /// _course Access Service.
    /// </summary>
    private readonly ICourseAccessService _courseAccessService;
    /// <summary>
    /// _publisher.
    /// </summary>
    private readonly IPublisher _publisher;

    public SendCourseGroupMessageCommandHandler(IUnitOfWork unitOfWork, ICourseAccessService courseAccessService, IPublisher publisher)
    {
        _unitOfWork = unitOfWork;
        _courseAccessService = courseAccessService;
        _publisher = publisher;
    }

    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<int> Handle(SendCourseGroupMessageCommand request, CancellationToken cancellationToken)
    {
        // Authorization: only Doctor/TA can send
        if (!await _courseAccessService.CanSendAsync(request.SenderUserId, request.CourseOfferingId))
        {
            throw new ForbiddenAccessException("You do not have permission to send messages in this course.");
        }

        var message = new ATLAS.Domain.Entities.CourseGroupMessage
        {
            CourseOfferingID = request.CourseOfferingId,
            SenderUserID = request.SenderUserId,
            Content = request.Content,
            SentAt = DateTime.UtcNow,
            IsPinned = false,
            IsDeleted = false
        };

        await _unitOfWork.CourseGroupMessages.AddAsync(message);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish domain event to create notifications
        await _publisher.Publish(new CourseGroupMessageSentEvent(message.MessageID, request.CourseOfferingId, request.SenderUserId,request.Content), cancellationToken);

        return message.MessageID;
    }
}
