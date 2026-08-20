using ATLAsBLL.Events;
using ATLASDomain.Entities;
using ATLAsDCL.UnitOfWork;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ATLAS.Domain.Entities;

namespace ATLAsBLL.Services.Notifications
{
    public class CreateNotificationsOnMessageSent : INotificationHandler<CourseGroupMessageSentEvent>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateNotificationsOnMessageSent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CourseGroupMessageSentEvent notification, CancellationToken cancellationToken)
        {
      var studentUserIds = await _unitOfWork.Enrollments
                             .GetStudentUserIdsByCourseOfferingAsync(notification.CourseOfferingId);
            var offering = (await _unitOfWork.CourseOfferings
                          .GetCourseOFferingByID( notification.CourseOfferingId));

            if (offering == null)
            {
                // Handle the case where the course offering is not found
                return;
            }
            var title = offering.Course.C_Name;

            var notifications = new List<Notification>();
            foreach (var uid in studentUserIds)
            {
                notifications.Add(new Notification
                {
                    RecipientUserID = uid,
                    Type = "CourseGroupMessage",
                    Title = title,
                    Body = notification.Content,
                    RelatedMessageID = notification.MessageId,
                    IsRead = false
                });
            }

            if (notifications.Any())
            {
                await _unitOfWork.Notifications.AddRangeAsync(notifications);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
