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
    /// Course Group Message Repository.
    /// </summary>
    public class CourseGroupMessageRepository : GenericRepository<CourseGroupMessage>, ICourseGroupMessageRepository
    {
        /// <summary>
        /// _Context.
        /// </summary>
        private readonly AtLASDbContext _Context;
        /// <summary>
        /// base.
        /// </summary>
        public CourseGroupMessageRepository(AtLASDbContext context) : base(context)
        {
            _Context = context;
        }


      

        /// <summary>
        /// Get By Course Offering Async.
        /// </summary>
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

        /// <summary>
        /// Soft Delete Async.
        /// </summary>
        public Task SoftDeleteAsync(CourseGroupMessage message)
        {
            message.IsDeleted = true;
            _Context.CourseGroupMessages.Update(message);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Toggle Pin Async.
        /// </summary>
        public Task TogglePinAsync(CourseGroupMessage message)
        {
            message.IsPinned = !message.IsPinned;
            _Context.CourseGroupMessages.Update(message);
            return Task.CompletedTask;
        }
    }
}
