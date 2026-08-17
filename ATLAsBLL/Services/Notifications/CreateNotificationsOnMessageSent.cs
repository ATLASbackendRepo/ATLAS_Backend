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
            // get enrolled students for the course offering
            var enrollments = await _unitOfWork.Enrollments.FindAsync(e => e.CourseOfferingID == notification.CourseOfferingId);

            var studentUserIds = enrollments.Select(e => e.Student.UserId).Distinct().Where(id => id != notification.SenderUserId).ToList();

            // fetch the course offering and its course name
            var offering = (await _unitOfWork.CourseOfferings
                .FindAsync(co => co.CourseOfferingID == notification.CourseOfferingId))
                .FirstOrDefault();

            if(offering == null)
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
                    Body = null,
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
