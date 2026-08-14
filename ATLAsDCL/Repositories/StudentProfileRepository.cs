using ATLAS.Domain.Entities;

using ATLAsDCL.Data;
using ATLASDomain.InterFaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ATLAS.Persistence.Repositories
{
    public class StudentProfileRepository : IStudentProfileRepository
    {
        private readonly AtLASDbContext _context;

        public StudentProfileRepository(AtLASDbContext context)
        {
            _context = context;
        }

        public async Task<StudentProfile?> GetByStudentIdAsync(int studentId)
        {
            return await _context.StudentProfiles
                .AsNoTracking()
                .Where(p => p.Student.StudentId == studentId)
                .Select(p => new StudentProfile
                {
                    StudentProfileID = p.StudentProfileID,
                    StudentID = p.StudentID,
                    LinkedInUrl = p.LinkedInUrl,
                    SectionNumber = p.SectionNumber,
                    Faculty = p.Faculty,
                    QRCodeUrl = p.QRCodeUrl,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    Student = p.Student  
                })
                .FirstOrDefaultAsync();
        }
    }
}