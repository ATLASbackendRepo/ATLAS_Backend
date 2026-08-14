
using ATLAS.Domain.Entities;
using ATLAsDCL.Data;
using Microsoft.EntityFrameworkCore;

namespace ATLAS.Persistence.Repositories
{
    public class StaffProfileRepository : IStaffProfileRepository
    {
        private readonly AtLASDbContext _context;

        public StaffProfileRepository(AtLASDbContext context)
        {
            _context = context;
        }

        public async Task<StaffProfile?> GetByUserIdAsync(int userId)
        {
            return await _context.StaffProfiles
                .AsNoTracking()
                .Where(p => p.UserID == userId)
                .Select(p => new StaffProfile
                {
                    StaffProfileID = p.StaffProfileID,
                    UserID = p.UserID,
                    Role = p.Role,
                    DepartmentID = p.DepartmentID,
                    TeachingYears = p.TeachingYears,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    User = p.User
                })
                .FirstOrDefaultAsync();
        }
    }
}