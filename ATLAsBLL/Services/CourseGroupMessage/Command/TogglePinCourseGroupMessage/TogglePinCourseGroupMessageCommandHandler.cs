using MediatR;
using ATLAsBLL.Exceptions;
using ATLAsDCL.UnitOfWork;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.CourseGroupMessage.Command.TogglePinCourseGroupMessage;

public class TogglePinCourseGroupMessageCommandHandler : IRequestHandler<TogglePinCourseGroupMessageCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;

    public TogglePinCourseGroupMessageCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

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
