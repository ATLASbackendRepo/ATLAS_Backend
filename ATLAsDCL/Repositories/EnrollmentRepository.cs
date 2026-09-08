using ATLAsDCL.Data;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
using ATLASDomain.Views;
using Microsoft.EntityFrameworkCore;

public class EnrollmentRepository
    : GenericRepository<Enrollment>, IEnrollmentRepository
{
    public EnrollmentRepository(AtLASDbContext context)
        : base(context)
    {
    }

    public async Task<Enrollment?> GetEnrollmentDetailsAsync(int id)
    {
        return await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.CourseOffering)
            .FirstOrDefaultAsync(e => e.EnrollmentID == id);
    }

    public async Task<bool> IsStudentEnrolledAsync(int studentId, int courseOfferingId, DateTime semester)
    {
        return await _context.Enrollments
    .AsNoTracking()
    .AnyAsync(e =>
        e.StudentID == studentId &&
        e.CourseOfferingID == courseOfferingId &&
        e.CourseOffering.Semester.AcademicYear == semester.Date.Year.ToString());
    }

    public async Task<IEnumerable<Enrollment>> GetStudentEnrollmentsAsync(int studentId)
    {
        return await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.CourseOffering)
            .ThenInclude(co => co.Course)
            .Where(e => e.StudentID == studentId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Enrollment>> GetCourseEnrollmentsAsync(int courseOfferingId)
    {
        return await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Student)
            .Where(e => e.CourseOfferingID == courseOfferingId)
            .ToListAsync();
    }

    public async Task<SemesterStudentRankingView?> GetStudentSemesterRankAsync(
      int studentId, int semesterId, CancellationToken cancellationToken)
    {
        return await _context.SemesterStudentRankings
            .AsNoTracking()
            .Where(r => r.SemesterID == semesterId && r.StudentID == studentId )
            .FirstOrDefaultAsync();
    }

    public async Task<List<SemesterStudentRankingView>> GetTopRankedStudentsInSemesterAsync(
        int semesterId,int AcademicLevel ,int topCount, CancellationToken cancellationToken)
    {
        return await _context.SemesterStudentRankings
            .AsNoTracking()
            .Where(r => r.SemesterID == semesterId && r.AcademicLevel == AcademicLevel && r.Rank <= topCount)
            .OrderBy(r => r.Rank)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<int?>> GetStudentUserIdsByCourseOfferingAsync(int courseOfferingId)
    {
        return await _context.Enrollments
            .Where(e => e.CourseOfferingID == courseOfferingId)
            .Select(e => e.Student.UserId)
            .Distinct()
            .ToListAsync();
    }



}
