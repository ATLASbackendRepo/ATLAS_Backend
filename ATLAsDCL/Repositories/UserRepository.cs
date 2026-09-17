using ATLAsDCL.Data;
using ATLASDomain.DTO.Staff;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

public class UserRepository
    : GenericRepository<User>, IUserRepository
{
    public UserRepository(AtLASDbContext context)
        : base(context)
    {
    }

    public async Task<User?> GetByUserNameAsync(string userName)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.FullName == userName);
    }
    public async Task<User?> GetByUserIdAsync(int userId)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.UserId == userId);
    }


    public async Task<User?> GetUserByAcademicIDAsync(string academicID)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u=> u.AcademicId  == academicID);
    }


    public async Task<bool> UserNameExistsAsync(string userName)
    {
        return await _dbSet.AnyAsync(u => u.FullName == userName);
    }

    public async Task<IEnumerable<StaffDto>> GetAllStaffAsync(enUserRole role, StaffFilterDTO filter)
    {
        var query = _context.Users
            .AsNoTracking()
            .AsQueryable();

        if(!string.IsNullOrEmpty(filter.Search))
        {
            query = query.Where(u => u.FullName.Contains(filter.Search) || (u.AcademicId != null && u.AcademicId.Contains(filter.Search)));
        }
        if (filter.DepartmentId.HasValue)
        {
            query = query.Where(u => u.DepartmentId == filter.DepartmentId.Value);
        }
        if (filter.Status.HasValue)
        {
            query = query.Where(u => u.IsActive == filter.Status.Value);
        }

        var staffList = query
            .Where(u => u.Role == role)
            .Select(u => new StaffDto
            {
                FullName = u.FullName,
                AcadimicId = u.AcademicId,

                NumberOfCourses = role == enUserRole.Doctor
                    ? _context.CourseOfferingDoctors
                        .Count(c => c.DoctorID == u.UserId)

                    : role == enUserRole.TeachingAssistant
                        ? _context.CourseOfferingTAs
                            .Count(c => c.TeacherAssistID == u.UserId)

                        : 0,

                departmentname = u.Department != null ? u.Department.D_Name : string.Empty,
                IsActive = u.IsActive
            });

        return await staffList.ToListAsync();
    }
  

    public async Task<int?> GetUserIdByAcdimicId(string AcdimicID, CancellationToken cancellationToken)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.AcademicId == AcdimicID)
            .Select(u => (int?)u.UserId)
            .FirstOrDefaultAsync(cancellationToken);
     
    }
}
