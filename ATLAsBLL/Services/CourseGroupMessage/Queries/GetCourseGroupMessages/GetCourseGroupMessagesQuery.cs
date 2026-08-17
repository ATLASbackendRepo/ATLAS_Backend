using ATLAsBLL.Services.CourseGroupMessage.DTOs;
using MediatR;

namespace ATLAsBLL.Services.CourseGroupMessage.Queries.GetCourseGroupMessages;

public record GetCourseGroupMessagesQuery(
    int RequestingUserId,
    int CourseOfferingId,
    int Page = 1,
    int PageSize = 20
) : IRequest<List<CourseGroupMessageDto>>;