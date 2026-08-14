using MediatR;
using ATLAsBLL.Exceptions;
using ATLAsDCL.UnitOfWork;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.CourseGroupMessage.Command.DeleteCourseGroupMessage;

public class DeleteCourseGroupMessageCommandHandler : IRequestHandler<DeleteCourseGroupMessageCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCourseGroupMessageCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

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
