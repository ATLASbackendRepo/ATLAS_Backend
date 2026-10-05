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
    /// <summary>
    /// Create Notifications On Message Sent.
    /// </summary>
    public class CreateNotificationsOnMessageSent : INotificationHandler<CourseGroupMessageSentEvent>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;
        /// <summary>
        /// Body Preview Length.
        /// </summary>
        private const int BodyPreviewLength = 500;

        public CreateNotificationsOnMessageSent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task Handle(CourseGroupMessageSentEvent notification, CancellationToken cancellationToken)
        {
            var preview = notification.Content.Length <= BodyPreviewLength
        ? notification.Content
        : notification.Content[
            ..(BodyPreviewLength - 1)] + "…";




            var studentUserIds = await _unitOfWork.Enrollments
                             .GetStudentUserIdsByCourseOfferingAsync(notification.CourseOfferingId, cancellationToken);
            var offering = (await _unitOfWork.CourseOfferings
                          .GetCourseOFferingByID( notification.CourseOfferingId, cancellationToken));

            if (offering == null)
            {
                // Handle the case where the course offering is not found
                return;
            }
            var title = offering.Course.C_Name;






            var notifications = new List<Notification>();
            foreach (var uid in studentUserIds)
            {
                if(uid == null ) { continue; }

                notifications.Add(new Notification
                {
                    RecipientUserID = (int) uid,
                    Type = "CourseGroupMessage",
                    Title = title,
                    Body = preview,
                    RelatedMessageID = notification.MessageId,
                    IsRead = false
                });
            }

            if (notifications.Any())
            {
                await _unitOfWork.Notifications.AddRangeAsync(notifications, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
