using ATLAsDCL.Data;
using ATLASDomain.DTO.Department;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Department Repository.
/// </summary>
public class DepartmentRepository
    : GenericRepository<Department>, IDepartmentRepository
{
    public DepartmentRepository(AtLASDbContext context)
        : base(context)
    {

    }
    /// <summary>
    /// Get Department With Students Async.
    /// </summary>
    public async Task<Department?> GetDepartmentWithStudentsAsync(int departmentId)
    {
        return await _context.Departments
            .AsNoTracking()
            .Include(d => d.Students)
            .ThenInclude(s => s.User)
            .FirstOrDefaultAsync(d => d.DepartmentId == departmentId);
    }

    /// <summary>
    /// Get Department With Doctors Async.
    /// </summary>
    public async Task<Department?> GetDepartmentWithDoctorsAsync(int departmentId)
    {
        return await _context.Departments
            .AsNoTracking()
            .Include(d => d.Users)
            .FirstOrDefaultAsync(d => d.DepartmentId == departmentId);
    }

    /// <summary>
    /// Get Department Withname Async.
    /// </summary>
    public async Task<int> GetDepartmentWithnameAsync(string departmentName, int institutionId)
    {
        var departmentId = await _context.Departments
            .AsNoTracking()
            .Where(d => d.D_Name == departmentName && d.InstitutionID == institutionId)
            .Select(d => (int?)d.DepartmentId)
            .FirstOrDefaultAsync();

        if (departmentId.HasValue)
        {
            return departmentId.Value;
        }

       var newDepartment = new Department
       {
           D_Name = departmentName,
           InstitutionID = institutionId
       };
        _context.Departments.Add(newDepartment);
        await _context.SaveChangesAsync();
        return newDepartment.DepartmentId;
    }

    /// <summary>
    /// Get Departments.
    /// </summary>
    public async Task<List<DepartmentDTO>> GetDepartments(string? departmentName)
    {
        var query = _context.Departments
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(departmentName))
        {
            var searchTerm = $"%{departmentName}%";
            query = query.Where(d => EF.Functions.Like(d.D_Name, searchTerm));
        }


        var departments = await query
       .Select(d => new DepartmentDTO
            {
                Id = d.DepartmentId,
                DepartmentName = d.D_Name,
                NumOfStudents = d.Students.Count,
                NumOfDoctors = d.Users.Count(u => u.Role == enUserRole.Doctor),
                NumOfTeacherAssistants = d.Users.Count(u => u.Role == enUserRole.TeachingAssistant)
            })
            .ToListAsync();

        return departments;
    }
}
