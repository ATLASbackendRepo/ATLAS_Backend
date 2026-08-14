using ATLAsBLL.CQRS;

namespace ATLAsBLL.Services.CourseGroupMessage.Queries.GetCourseGroupMessages;

public record GetCourseGroupMessagesQuery(int CourseOfferingId, int RequestingUserId, int Page = 1, int PageSize = 20) : IQuery<string>;
