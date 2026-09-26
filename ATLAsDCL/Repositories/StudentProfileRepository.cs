
using System.Linq.Expressions;
using ATLAS.Domain.Entities;
using ATLAsDCL.Data;
using ATLASDomain.InterFaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ATLAS.Persistence.Repositories
{
    /// <summary>
    /// Student Profile Repository.
    /// </summary>
    public class StudentProfileRepository : IStudentProfileRepository, IGenericRepository<StudentProfile>
    {
        /// <summary>
        /// _context.
        /// </summary>
        private readonly AtLASDbContext _context;

        public StudentProfileRepository(AtLASDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get By User Id Async.
        /// </summary>
        /// <remarks>
        /// The profile, its student and that student's user are returned tracked so
        /// a caller that edits them only writes the columns it actually changed. A
        /// detached graph forces the caller to call Update, which rewrites every
        /// property, including the user's password and refresh-token hash.
        /// </remarks>
        public async Task<StudentProfile?> GetByUserIdAsync(int studentId)
        {
            return await _context.StudentProfiles
                .Include(p => p.Student)
                    .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(p => p.StudentID == studentId);
        }

        // IGenericRepository<StudentProfile> implementations

        /// <summary>
        /// Get By Id Async.
        /// </summary>
        public async Task<StudentProfile?> GetByIdAsync(int id)
        {
            return await _context.StudentProfiles.FindAsync(id);
        }

        /// <summary>
        /// Get All Async.
        /// </summary>
        public async Task<IEnumerable<StudentProfile>> GetAllAsync()
        {
            return await _context.StudentProfiles.AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// Find Async.
        /// </summary>
        public async Task<IEnumerable<StudentProfile>> FindAsync(Expression<Func<StudentProfile, bool>> predicate)
        {
            return await _context.StudentProfiles.Where(predicate).AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// First Or Default Async.
        /// </summary>
        public async Task<StudentProfile?> FirstOrDefaultAsync(Expression<Func<StudentProfile, bool>> predicate)
        {
            return await _context.StudentProfiles.AsNoTracking().FirstOrDefaultAsync(predicate);
        }

        /// <summary>
        /// Any Async.
        /// </summary>
        public async Task<bool> AnyAsync(Expression<Func<StudentProfile, bool>> predicate)
        {
            return await _context.StudentProfiles.AnyAsync(predicate);
        }

        /// <summary>
        /// Count Async.
        /// </summary>
        public async Task<int> CountAsync()
        {
            return await _context.StudentProfiles.CountAsync();
        }

        /// <summary>
        /// Count Async.
        /// </summary>
        public async Task<int> CountAsync(Expression<Func<StudentProfile, bool>> predicate)
        {
            return await _context.StudentProfiles.CountAsync(predicate);
        }

        /// <summary>
        /// Add Async.
        /// </summary>
        public async Task AddAsync(StudentProfile entity)
        {
            await _context.StudentProfiles.AddAsync(entity);
        }

        /// <summary>
        /// Add Range Async.
        /// </summary>
        public async Task AddRangeAsync(IEnumerable<StudentProfile> entities)
        {
            await _context.AddRangeAsync(entities);
        }

        /// <summary>
        /// Update.
        /// </summary>
        public void Update(StudentProfile entity)
        {
            _context.StudentProfiles.Update(entity);
        }

        /// <summary>
        /// Delete.
        /// </summary>
        public void Delete(StudentProfile entity)
        {
            _context.StudentProfiles.Remove(entity);
        }

        /// <summary>
        /// Delete Range.
        /// </summary>
        public void DeleteRange(IEnumerable<StudentProfile> entities)
        {
            _context.StudentProfiles.RemoveRange(entities);
        }
    }
}
