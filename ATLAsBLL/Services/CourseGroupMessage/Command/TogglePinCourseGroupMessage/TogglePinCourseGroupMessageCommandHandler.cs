using MediatR;
using ATLAsBLL.Exceptions;
using ATLAsDCL.UnitOfWork;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.CourseGroupMessage.Command.TogglePinCourseGroupMessage;

/// <summary>
/// Toggle Pin Course Group Message Command Handler.
/// </summary>
public class TogglePinCourseGroupMessageCommandHandler : IRequestHandler<TogglePinCourseGroupMessageCommand, int>
{
    /// <summary>
    /// _unit Of Work.
    /// </summary>
    private readonly IUnitOfWork _unitOfWork;

    public TogglePinCourseGroupMessageCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<int> Handle(TogglePinCourseGroupMessageCommand request, CancellationToken cancellationToken)
    {
        var message = await _unitOfWork.CourseGroupMessages.GetByIdAsync(request.MessageId);

        if (message == null)
        {
            throw new KeyNotFoundException($"Message with ID {request.MessageId} not found.");
        }

        // Authorization: only sender can toggle pin
        if (message.SenderUserID != request.RequestingUserId)
        {
            throw new ForbiddenAccessException("You can only pin/unpin your own messages.");
        }

        await _unitOfWork.CourseGroupMessages.TogglePinAsync(message);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return message.MessageID;
    }
}
