using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ATLAS.Domain.Entities;
using ATLAsDCL.Data;
using ATLASDomain.InterFaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ATLAS.Persistence.Repositories
{
    public class StaffProfileRepository : IStaffProfileRepository, IGenericRepository<StaffProfile>
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
                    User = p.User
                })
                .FirstOrDefaultAsync();
        }

        // IGenericRepository<StaffProfile> implementations
        public async Task<StaffProfile?> GetByIdAsync(int id)
        {
            return await _context.StaffProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.StaffProfileID == id);
        }

        public async Task<IEnumerable<StaffProfile>> GetAllAsync()
        {
            return await _context.StaffProfiles
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<StaffProfile>> FindAsync(Expression<Func<StaffProfile, bool>> predicate)
        {
            return await _context.StaffProfiles
                .AsNoTracking()
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<StaffProfile?> FirstOrDefaultAsync(Expression<Func<StaffProfile, bool>> predicate)
        {
            return await _context.StaffProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(predicate);
        }

        public async Task<bool> AnyAsync(Expression<Func<StaffProfile, bool>> predicate)
        {
            return await _context.StaffProfiles
                .AsNoTracking()
                .AnyAsync(predicate);
        }

        public async Task<int> CountAsync()
        {
            return await _context.StaffProfiles
                .AsNoTracking()
                .CountAsync();
        }

        public async Task<int> CountAsync(Expression<Func<StaffProfile, bool>> predicate)
        {
            return await _context.StaffProfiles
                .AsNoTracking()
                .CountAsync(predicate);
        }

        public async Task AddAsync(StaffProfile entity)
        {
            await _context.StaffProfiles.AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<StaffProfile> entities)
        {
            await _context.StaffProfiles.AddRangeAsync(entities);
        }

        public void Update(StaffProfile entity)
        {
            _context.StaffProfiles.Update(entity);
        }

        public void Delete(StaffProfile entity)
        {
            _context.StaffProfiles.Remove(entity);
        }

        public void DeleteRange(IEnumerable<StaffProfile> entities)
        {
            _context.StaffProfiles.RemoveRange(entities);
        }
    }
}