using ATLAS.Domain.Entities;
using ATLASDomain.Entities;
using ATLASDomain.InterFaces.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ATLASDomain.Interfaces.Repositories
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetUnreadForUserAsync(int userId);
        
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadAsync(int userId);
    }
}
