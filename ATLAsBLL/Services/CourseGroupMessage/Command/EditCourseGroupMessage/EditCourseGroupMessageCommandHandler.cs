using MediatR;
using ATLAsBLL.Exceptions;
using ATLAsDCL.UnitOfWork;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.CourseGroupMessage.Command.EditCourseGroupMessage;

/// <summary>
/// Edit Course Group Message Command Handler.
/// </summary>
public class EditCourseGroupMessageCommandHandler : IRequestHandler<EditCourseGroupMessageCommand, int>
{
    /// <summary>
    /// _unit Of Work.
    /// </summary>
    private readonly IUnitOfWork _unitOfWork;

    public EditCourseGroupMessageCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<int> Handle(EditCourseGroupMessageCommand request, CancellationToken cancellationToken)
    {
        var message = await _unitOfWork.CourseGroupMessages.GetByIdAsync(request.MessageId);

        if (message == null)
        {
            throw new KeyNotFoundException($"Message with ID {request.MessageId} not found.");
        }

        // Authorization: only sender can edit
        if (message.SenderUserID != request.RequestingUserId)
        {
            throw new ForbiddenAccessException("You can only edit your own messages.");
        }

        message.Content = request.NewContent;
        _unitOfWork.CourseGroupMessages.Update(message);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return message.MessageID;
    }
}
