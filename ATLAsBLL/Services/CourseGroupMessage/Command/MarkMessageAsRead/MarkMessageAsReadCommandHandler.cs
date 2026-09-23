using MediatR;
using ATLAsBLL.Exceptions;
using ATLAsDCL.UnitOfWork;
using ATLAS.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.CourseGroupMessage.Command.MarkMessageAsRead;

/// <summary>
/// Mark Message As Read Command Handler.
/// </summary>
public class MarkMessageAsReadCommandHandler : IRequestHandler<MarkMessageAsReadCommand, int>
{
    /// <summary>
    /// _unit Of Work.
    /// </summary>
    private readonly IUnitOfWork _unitOfWork;

    public MarkMessageAsReadCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<int> Handle(MarkMessageAsReadCommand request, CancellationToken cancellationToken)
    {
        var message = await _unitOfWork.CourseGroupMessages.GetByIdAsync(request.MessageId);

        if (message == null)
        {
            throw new KeyNotFoundException($"Message with ID {request.MessageId} not found.");
        }

        if (request.Role == "Student")
        {

            var isEnrolled = await _unitOfWork.Enrollments.FirstOrDefaultAsync(e =>
                e.CourseOfferingID == message.CourseOfferingID && e.Student.UserId == request.UserId);

            if (isEnrolled == null)
            {
                throw new ForbiddenAccessException("You are not enrolled in this course.");
            }
        }
        else if (request.Role == "Doctor")
        {
            var isInstructor = await _unitOfWork.CourseOfferingDoctors.FirstOrDefaultAsync(co =>
                co.CourseOfferingID == message.CourseOfferingID && co.Doctor.UserId == request.UserId);
            if (isInstructor == null)
            {
                throw new ForbiddenAccessException("You are not the instructor for this course.");
            }
        }
        else if (request.Role == "TeachingAssistant")
        {
            var isteacherassist = await _unitOfWork.CourseOfferingTAs.FirstOrDefaultAsync(co =>
                co.CourseOfferingID == message.CourseOfferingID && co.TeachingAssistant.UserId == request.UserId);
            if (isteacherassist == null)
            {
                throw new ForbiddenAccessException("You are not the instructor for this course.");
            }
        }
        else
        {
            throw new ForbiddenAccessException("Invalid user role.");
        }
        // Write read receipt
        var readReceipt = new CourseGroupMessageRead
        {
            MessageID = request.MessageId,
            UserID = request.UserId,
            ReadAt = DateTime.UtcNow
        };

        await _unitOfWork.CourseGroupMessagesRead.AddAsync(readReceipt );
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return request.MessageId;
    }
}
