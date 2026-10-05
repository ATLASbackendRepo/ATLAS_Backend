using ATLAsDCL.Data;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Course Offering Doctor Repository.
/// </summary>
public class CourseOfferingDoctorRepository
    : GenericRepository<CourseOfferingDoctor>, ICourseOfferingDoctorRepository
{
    public CourseOfferingDoctorRepository(AtLASDbContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Get By Doctor Async.
    /// </summary>
    public async Task<IEnumerable<CourseOfferingDoctor>> GetByDoctorAsync(int doctorId, CancellationToken cancellationToken = default)
    {
        return await _context.CourseOfferingDoctors
            .AsNoTracking()
            .Include(c => c.CourseOffering)
            .Where(c => c.DoctorID == doctorId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get Doctors By Offering Async.
    /// </summary>
    public async Task<IEnumerable<int>> GetDoctorsByOfferingAsync(int offeringId, CancellationToken cancellationToken = default)
    {
        return await _context.CourseOfferingDoctors
       .AsNoTracking()
       .Where(c => c.CourseOfferingID == offeringId)
       .Select(c => c.DoctorID)
       .Distinct()
       .ToListAsync(cancellationToken);
    }
    /// <summary>
    /// Get Doctor Ids By Course Offering Id Async.
    /// </summary>
    public async Task<IEnumerable<int>> GetDoctorIdsByCourseOfferingIdAsync(int offeringId, CancellationToken cancellationToken = default)
    {
        return await _context.CourseOfferingDoctors
            .AsNoTracking()
            .Include(c => c.CourseOffering)
            .Where(c => c.CourseOfferingID == offeringId)
            .Select(c => c.DoctorID)
            .ToListAsync(cancellationToken);
    }
}
