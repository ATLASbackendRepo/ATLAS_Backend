using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
using ATLAsDCL.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ATLAS.Domain.Entities;

namespace ATLAsDCL.Repositories
{
    /// <summary>
    /// Notification Repository.
    /// </summary>
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        /// <summary>
        /// base.
        /// </summary>
        public NotificationRepository(AtLASDbContext context) : base(context)
        {
        }

      

        /// <summary>
        /// Get Unread For User Async.
        /// </summary>
        public async Task<IEnumerable<Notification>> GetUnreadForUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet.Where(n => n.RecipientUserID == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Mark All As Read Async.
        /// </summary>
        /// <returns>The number of notifications that were actually marked.</returns>
        public async Task<int> MarkAllAsReadAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet.Where(n => n.RecipientUserID == userId && !n.IsRead)
                .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.IsRead, true), cancellationToken);
        }

        /// <summary>
        /// Mark As Read Async.
        /// </summary>
        public async Task MarkAsReadAsync(int notificationId, CancellationToken cancellationToken = default)
        {
            var item = await _dbSet.FindAsync(notificationId, cancellationToken);
            if (item != null)
            {
                item.IsRead = true;
                _dbSet.Update(item);
            }
        }
    }
}
