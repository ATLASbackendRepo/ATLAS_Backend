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

      

        public async Task<IEnumerable<Notification>> GetUnreadForUserAsync(int userId)
        {
            return await _dbSet.Where(n => n.RecipientUserID == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            await _dbSet.Where(n => n.RecipientUserID == userId && !n.IsRead)
                .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.IsRead, true));
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
