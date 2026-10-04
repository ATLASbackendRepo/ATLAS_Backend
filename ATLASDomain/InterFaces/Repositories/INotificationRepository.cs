using ATLAS.Domain.Entities;
using ATLASDomain.Entities;
using ATLASDomain.InterFaces.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ATLASDomain.Interfaces.Repositories
{
    /// <summary>
    /// INotification Repository.
    /// </summary>
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        /// <summary>
        /// Gets the unread notifications of a user, newest first.
        /// </summary>
        /// <param name="userId">User identifier.</param>
        /// <returns>The unread notifications.</returns>
        Task<IEnumerable<Notification>> GetUnreadForUserAsync(int userId);

        /// <summary>
        /// Marks a single notification as read.
        /// </summary>
        /// <param name="notificationId">Notification identifier.</param>
        Task MarkAsReadAsync(int notificationId);

        /// <summary>
        /// Marks every notification of a user as read.
        /// </summary>
        /// <param name="userId">User identifier.</param>
        /// <returns>The number of notifications updated.</returns>
        Task<int> MarkAllAsReadAsync(int userId);
    }
}
