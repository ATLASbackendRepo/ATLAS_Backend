using MediatR;
using Microsoft.EntityFrameworkCore;
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
        var message = await _unitOfWork.CourseGroupMessages.GetByIdAsync(request.MessageId, cancellationToken);

        if (message == null || message.IsDeleted)
        {
            throw new KeyNotFoundException($"Message with ID {request.MessageId} not found.");
        }

        if (request.Role == "Student")
        {

            var isEnrolled = await _unitOfWork.Enrollments.FirstOrDefaultAsync(e =>
                e.CourseOfferingID == message.CourseOfferingID && e.Student.UserId == request.UserId, cancellationToken);

            if (isEnrolled == null)
            {
                throw new ForbiddenAccessException("You are not enrolled in this course.");
            }
        }
        else if (request.Role == "Doctor")
        {
            var isInstructor = await _unitOfWork.CourseOfferingDoctors.FirstOrDefaultAsync(co =>
                co.CourseOfferingID == message.CourseOfferingID && co.Doctor.UserId == request.UserId, cancellationToken);
            if (isInstructor == null)
            {
                throw new ForbiddenAccessException("You are not the instructor for this course.");
            }
        }
        else if (request.Role == "TeachingAssistant")
        {
            var isteacherassist = await _unitOfWork.CourseOfferingTAs.FirstOrDefaultAsync(co =>
                co.CourseOfferingID == message.CourseOfferingID && co.TeachingAssistant.UserId == request.UserId, cancellationToken);
            if (isteacherassist == null)
            {
                throw new ForbiddenAccessException("You are not the instructor for this course.");
            }
        }
        else
        {
            throw new ForbiddenAccessException("Invalid user role.");
        }
        // The read receipt is keyed by (MessageID, UserID), so marking the same
        // message read twice is not an error: the caller's intent is already
        // satisfied and a second insert would only raise a duplicate key (LOW-11).
        var alreadyRead = await _unitOfWork.CourseGroupMessagesRead.AnyAsync(
            receipt => receipt.MessageID == request.MessageId &&
                       receipt.UserID == request.UserId, cancellationToken);

        if (alreadyRead)
        {
            return request.MessageId;
        }

        // Write read receipt
        var readReceipt = new CourseGroupMessageRead
        {
            MessageID = request.MessageId,
            UserID = request.UserId,
            ReadAt = DateTime.UtcNow
        };

        await _unitOfWork.CourseGroupMessagesRead.AddAsync(readReceipt , cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // A concurrent request inserted the receipt first, which is the same
            // outcome this request wanted. Anything else is a real failure.
            var persisted = await _unitOfWork.CourseGroupMessagesRead.AnyAsync(
                receipt => receipt.MessageID == request.MessageId &&
                           receipt.UserID == request.UserId, cancellationToken);

            if (!persisted)
            {
                throw;
            }
        }

        return request.MessageId;
    }
}
