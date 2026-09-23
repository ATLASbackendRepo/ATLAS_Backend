using ATLAS.Domain.Entities;
using ATLAsDCL.Data;
using ATLASDomain.DTO.Profile;
using ATLASDomain.InterFaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ATLAS.Persistence.Repositories
{
    /// <summary>
    /// Staff Profile Repository.
    /// </summary>
    public class StaffProfileRepository : IStaffProfileRepository, IGenericRepository<StaffProfile>
    {
        /// <summary>
        /// _context.
        /// </summary>
        private readonly AtLASDbContext _context;

        public StaffProfileRepository(AtLASDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get By User Id Async.
        /// </summary>
        public async Task<StaffProfile?> GetByUserIdAsync(int userId)
        {
            return await _context.StaffProfiles
                .AsNoTracking()
                .Where(p => p.UserID == userId)
                .Select(p => new StaffProfile
                {
                    StaffProfileID = p.StaffProfileID,
                    AcademicId = p.AcademicId,
                    Role = p.Role,
                    DepartmentID = p.DepartmentID,
                    TeachingYears = p.TeachingYears,
                    User = p.User

                })
                .FirstOrDefaultAsync();
        }

        // IGenericRepository<StaffProfile> implementations
        /// <summary>
        /// Get By Id Async.
        /// </summary>
        public async Task<StaffProfile?> GetByIdAsync(int id)
        {
            return await _context.StaffProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.StaffProfileID == id);
        }

        /// <summary>
        /// Get All Async.
        /// </summary>
        public async Task<IEnumerable<StaffProfile>> GetAllAsync()
        {
            return await _context.StaffProfiles
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Find Async.
        /// </summary>
        public async Task<IEnumerable<StaffProfile>> FindAsync(Expression<Func<StaffProfile, bool>> predicate)
        {
            return await _context.StaffProfiles
                .AsNoTracking()
                .Where(predicate)
                .ToListAsync();
        }

        /// <summary>
        /// First Or Default Async.
        /// </summary>
        public async Task<StaffProfile?> FirstOrDefaultAsync(Expression<Func<StaffProfile, bool>> predicate)
        {
            return await _context.StaffProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(predicate);
        }

        /// <summary>
        /// Any Async.
        /// </summary>
        public async Task<bool> AnyAsync(Expression<Func<StaffProfile, bool>> predicate)
        {
            return await _context.StaffProfiles
                .AsNoTracking()
                .AnyAsync(predicate);
        }

        /// <summary>
        /// Count Async.
        /// </summary>
        public async Task<int> CountAsync()
        {
            return await _context.StaffProfiles
                .AsNoTracking()
                .CountAsync();
        }

        /// <summary>
        /// Count Async.
        /// </summary>
        public async Task<int> CountAsync(Expression<Func<StaffProfile, bool>> predicate)
        {
            return await _context.StaffProfiles
                .AsNoTracking()
                .CountAsync(predicate);
        }

        /// <summary>
        /// Add Async.
        /// </summary>
        public async Task AddAsync(StaffProfile entity)
        {
            await _context.StaffProfiles.AddAsync(entity);
        }

        /// <summary>
        /// Add Range Async.
        /// </summary>
        public async Task AddRangeAsync(IEnumerable<StaffProfile> entities)
        {
            await _context.StaffProfiles.AddRangeAsync(entities);
        }

        /// <summary>
        /// Update.
        /// </summary>
        public void Update(StaffProfile entity)
        {
            _context.StaffProfiles.Update(entity);
        }

        /// <summary>
        /// Delete.
        /// </summary>
        public void Delete(StaffProfile entity)
        {
            _context.StaffProfiles.Remove(entity);
        }

        /// <summary>
        /// Delete Range.
        /// </summary>
        public void DeleteRange(IEnumerable<StaffProfile> entities)
        {
            _context.StaffProfiles.RemoveRange(entities);
        }
    }
}
