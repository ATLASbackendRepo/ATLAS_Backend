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
    public class CourseGroupMessageRepository : GenericRepository<CourseGroupMessage>, ICourseGroupMessageRepository
    {
        private readonly AtLASDbContext _Context;
        public CourseGroupMessageRepository(AtLASDbContext context) : base(context)
        {
            _Context = context;
        }

        public async Task AddAsync(CourseGroupMessage message)
        {
            await _Context.CourseGroupMessages.AddAsync(message);
        }

        public async Task<CourseGroupMessage?> GetByIdAsync(int messageId)
        {
            return await _Context.CourseGroupMessages
                .Include(m => m.SenderUser)
                .Include(m => m.ReadReceipts)
                .Include(m => m.Notifications)
                .FirstOrDefaultAsync(m => m.MessageID == messageId && !m.IsDeleted);
        }

        public async Task<IEnumerable<CourseGroupMessage>> GetByCourseOfferingAsync(int courseOfferingId, int page, int pageSize)
        {
            return await _Context.CourseGroupMessages
                .Where(m => m.CourseOfferingID == courseOfferingId && !m.IsDeleted)
                .OrderByDescending(m => m.IsPinned)
                .ThenByDescending(m => m.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(m => m.SenderUser)
                .ToListAsync();
        }

        public Task SoftDeleteAsync(CourseGroupMessage message)
        {
            message.IsDeleted = true;
            _Context.CourseGroupMessages.Update(message);
            return Task.CompletedTask;
        }

        public Task TogglePinAsync(CourseGroupMessage message)
        {
            message.IsPinned = !message.IsPinned;
            _Context.CourseGroupMessages.Update(message);
            return Task.CompletedTask;
        }
    }
}
