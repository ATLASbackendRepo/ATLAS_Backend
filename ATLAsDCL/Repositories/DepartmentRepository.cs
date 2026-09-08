using ATLAsDCL.Data;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

public class DepartmentRepository
    : GenericRepository<Department>, IDepartmentRepository
{
    public DepartmentRepository(AtLASDbContext context)
        : base(context)
    {

    }
    public async Task<Department?> GetDepartmentWithStudentsAsync(int departmentId)
    {
        return await _context.Departments
            .AsNoTracking()
            .Include(d => d.Students)
            .ThenInclude(s => s.User)
            .FirstOrDefaultAsync(d => d.DepartmentId == departmentId);
    }

    public async Task<Department?> GetDepartmentWithDoctorsAsync(int departmentId)
    {
        return await _context.Departments
            .AsNoTracking()
            .Include(d => d.Users)
            .FirstOrDefaultAsync(d => d.DepartmentId == departmentId);
    }

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
}