
using System.Linq.Expressions;
using ATLAS.Domain.Entities;
using ATLAsDCL.Data;
using ATLASDomain.InterFaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ATLAS.Persistence.Repositories
{
    public class StudentProfileRepository : IStudentProfileRepository, IGenericRepository<StudentProfile>
    {
        private readonly AtLASDbContext _context;

        public StudentProfileRepository(AtLASDbContext context)
        {
            _context = context;
        }

        public async Task<StudentProfile?> GetByUserIdAsync(int studentId)
        {
         
        
            return await _context.StudentProfiles
                .AsNoTracking()
                .Include(p => p.Student)
                    .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(p => p.StudentID == studentId);
       
        }

        // IGenericRepository<StudentProfile> implementations

        public async Task<StudentProfile?> GetByIdAsync(int id)
        {
            return await _context.StudentProfiles.FindAsync(id);
        }

        public async Task<IEnumerable<StudentProfile>> GetAllAsync()
        {
            return await _context.StudentProfiles.AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<StudentProfile>> FindAsync(Expression<Func<StudentProfile, bool>> predicate)
        {
            return await _context.StudentProfiles.Where(predicate).AsNoTracking().ToListAsync();
        }

        public async Task<StudentProfile?> FirstOrDefaultAsync(Expression<Func<StudentProfile, bool>> predicate)
        {
            return await _context.StudentProfiles.AsNoTracking().FirstOrDefaultAsync(predicate);
        }

        public async Task<bool> AnyAsync(Expression<Func<StudentProfile, bool>> predicate)
        {
            return await _context.StudentProfiles.AnyAsync(predicate);
        }

        public async Task<int> CountAsync()
        {
            return await _context.StudentProfiles.CountAsync();
        }

        public async Task<int> CountAsync(Expression<Func<StudentProfile, bool>> predicate)
        {
            return await _context.StudentProfiles.CountAsync(predicate);
        }

        public async Task AddAsync(StudentProfile entity)
        {
            await _context.StudentProfiles.AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<StudentProfile> entities)
        {
            await _context.AddRangeAsync(entities);
        }

        public void Update(StudentProfile entity)
        {
            _context.StudentProfiles.Update(entity);
        }

        public void Delete(StudentProfile entity)
        {
            _context.StudentProfiles.Remove(entity);
        }

        public void DeleteRange(IEnumerable<StudentProfile> entities)
        {
            _context.StudentProfiles.RemoveRange(entities);
        }
    }
}