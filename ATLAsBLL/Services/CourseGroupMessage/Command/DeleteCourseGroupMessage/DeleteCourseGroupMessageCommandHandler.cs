using MediatR;
using ATLAsBLL.Exceptions;
using ATLAsDCL.UnitOfWork;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.CourseGroupMessage.Command.DeleteCourseGroupMessage;

/// <summary>
/// Delete Course Group Message Command Handler.
/// </summary>
public class DeleteCourseGroupMessageCommandHandler : IRequestHandler<DeleteCourseGroupMessageCommand, int>
{
    /// <summary>
    /// _unit Of Work.
    /// </summary>
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCourseGroupMessageCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<int> Handle(DeleteCourseGroupMessageCommand request, CancellationToken cancellationToken)
    {
        var message = await _unitOfWork.CourseGroupMessages.GetByIdAsync(request.MessageId);

        if (message == null)
        {
            throw new KeyNotFoundException($"Message with ID {request.MessageId} not found.");
        }

        // Authorization: only sender can delete
        if (message.SenderUserID != request.RequestingUserId)
        {
            throw new ForbiddenAccessException("You can only delete your own messages.");
        }

        await _unitOfWork.CourseGroupMessages.SoftDeleteAsync(message);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return message.MessageID;
    }
}
