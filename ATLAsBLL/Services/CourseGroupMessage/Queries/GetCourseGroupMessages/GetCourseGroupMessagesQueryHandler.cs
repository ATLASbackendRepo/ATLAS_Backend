using ATLAsBLL.Services.CourseGroupMessage.DTOs;
using ATLAsDCL.UnitOfWork;
using MediatR;

namespace ATLAsBLL.Services.CourseGroupMessage.Queries.GetCourseGroupMessages;

/// <summary>
/// Get Course Group Messages Query Handler.
/// </summary>
public class GetCourseGroupMessagesQueryHandler
    : IRequestHandler<
        GetCourseGroupMessagesQuery,
        List<CourseGroupMessageDto>>
{
    /// <summary>
    /// _unit Of Work.
    /// </summary>
    private readonly IUnitOfWork _unitOfWork;
    /// <summary>
    /// _course Access Service.
    /// </summary>
    private readonly ATLASDomain.Services.ICourseAccessService _courseAccessService;

    public GetCourseGroupMessagesQueryHandler(
        IUnitOfWork unitOfWork,
        ATLASDomain.Services.ICourseAccessService courseAccessService)
    {
        _unitOfWork = unitOfWork;
        _courseAccessService = courseAccessService;
    }

    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<List<CourseGroupMessageDto>> Handle(
        GetCourseGroupMessagesQuery request,
        CancellationToken cancellationToken)
    {
        // Authorization
        var canRead = await _courseAccessService.CanReadAsync(
            request.RequestingUserId,
            request.CourseOfferingId);

        if (!canRead)
        {
            throw new Exceptions.ForbiddenAccessException(
                "You do not have permission to view messages in this course.");
        }

        // Get messages
        var messages = await _unitOfWork.CourseGroupMessages
            .GetByCourseOfferingAsync(
                request.CourseOfferingId,
                request.Page,
                request.PageSize);

        // Map to DTO
        return messages.Select(m => new CourseGroupMessageDto
        {
            MessageID = m.MessageID,
            CourseOfferingID = m.CourseOfferingID,
            SenderUserID = m.SenderUserID,
            SenderName = m.SenderUser?.FullName,
            Content = m.Content,
            SentAt = m.SentAt,
            IsPinned = m.IsPinned,
            IsDeleted = m.IsDeleted,
            ReadCount = m.ReadReceipts?.Count ?? 0
        }).ToList();
    }
}
