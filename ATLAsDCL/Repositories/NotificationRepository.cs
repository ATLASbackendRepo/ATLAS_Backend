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
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(AtLASDbContext context) : base(context)
        {
        }

        public async Task AddRangeAsync(IEnumerable<Notification> notifications)
        {
            await _dbSet.AddRangeAsync(notifications);
        }

        public async Task<IEnumerable<Notification>> GetUnreadForUserAsync(int userId)
        {
            return await _dbSet.Where(n => n.RecipientUserID == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public Task MarkAllAsReadAsync(int userId)
        {
            var items = _dbSet.Where(n => n.RecipientUserID == userId && !n.IsRead);
            foreach (var n in items)
            {
                n.IsRead = true;
            }
            _dbSet.UpdateRange(items);
            return Task.CompletedTask;
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var item = await _dbSet.FindAsync(notificationId);
            if (item != null)
            {
                item.IsRead = true;
                _dbSet.Update(item);
            }
        }
    }
}
