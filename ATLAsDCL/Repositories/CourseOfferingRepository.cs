using ATLAsDCL.Data;
using ATLASDomain.DTO.Analysis;
using ATLASDomain.DTO.CourseOffring;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

/// <summary>
/// Course Offering Repository.
/// </summary>
public class CourseOfferingRepository
    : GenericRepository<CourseOffering>, ICourseOfferingRepository
{
    public CourseOfferingRepository(AtLASDbContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Get Details Async.
    /// </summary>
    private async Task<CourseOffering?> GetDetailsAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.CourseOfferings
            .AsNoTracking()
            .Include(c => c.Course)
            .Include(c => c.Semester)
            .Include(c => c.CourseOfferingDoctors)
            .Include(c => c.CourseOfferingTAs)
            .Include(c => c.Enrollments)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.CourseOfferingID == id, cancellationToken);
    }




    /// <summary>
    /// Get Course OFfering By ID.
    /// </summary>
    public async Task<CourseOffering?> GetCourseOFferingByID(int id, CancellationToken cancellationToken = default)
    {
        return await _context.CourseOfferings
            .AsNoTracking()
            .Include(c => c.Course)
            .FirstOrDefaultAsync(c => c.CourseOfferingID == id, cancellationToken);
    }

    /// <summary>
    /// Get Offering Details Async.
    /// </summary>
    public async Task<CourseOffering?> GetOfferingDetailsAsync(int offeringId, CancellationToken cancellationToken = default)
    {

        return await GetDetailsAsync(offeringId, cancellationToken);
    }

    /// <summary>
    /// Get By Semester Async.
    /// </summary>
    public async Task<IEnumerable<CourseOffering>> GetBySemesterAsync(int semesterId, CancellationToken cancellationToken = default)
    {
        return await _context.CourseOfferings
            .AsNoTracking()
            .Where(c => c.SemesterID == semesterId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get By Course Async.
    /// </summary>
    public async Task<IEnumerable<CourseOffering>> GetByCourseAsync(int courseId, CancellationToken cancellationToken = default)
    {
        return await _context.CourseOfferings
            .AsNoTracking()
            .Where(c => c.CourseID == courseId)
            .ToListAsync(cancellationToken);
    }


    /// <summary>
    /// Get My Course Async.
    /// </summary>
    public async Task<IEnumerable<CourseOfferingDTO>> GetMyCourseAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.CourseOfferings
            .AsNoTracking()
           
            .Where(co => co.Enrollments.Any(e => e.Student.UserId == userId))
            .Select(co => new CourseOfferingDTO
            {
                CourseOfferingId = co.CourseOfferingID,
                CourseOfferingHours = co.Course.Credit_Hours,
                CourseName = co.Course.C_Name,
                Semester = co.Semester.SemesterName,

              
                Doctorname = co.CourseOfferingDoctors
                    .Select(cd => cd.Doctor.FullName)
                    .FirstOrDefault() ?? "No Doctor Assigned",

                CourseOfferingGrade = co.Enrollments
                    .Where(e => e.Student.UserId == userId)
                    .SelectMany(e => e.CourseGrades)
                    .Sum(cg => (decimal?)cg.StudentDegree) ?? 0m
            })
            .ToListAsync(cancellationToken);
    }
    /// <summary>
    /// Get Details Course Async.
    /// </summary>
    public async Task<IEnumerable< CourseofferingDetailsDTO>> GetDetailsCourseAsync(
        int courseOfferingId, CancellationToken cancellationToken = default)
    {
        return await _context.CourseDetails
            .AsNoTracking()
            .Where(cd => cd.CourseOfferingID == courseOfferingId )
            .Select(cd => new CourseofferingDetailsDTO
            {
                C_Name = cd.C_Name,
                Credit_Hours = Convert.ToInt16(cd.Credit_Hours),
                ModuleId = cd.ModuleId,
                ModuleName = cd.ModuleName,
                maxDegree = cd.maxDegree,
                studentdegree = cd.studentdegree
            })
            .ToListAsync(cancellationToken);
    }
    /// <summary>
    /// Get Staff My Course Async.
    /// </summary>
    public async Task<IEnumerable<DoctorCourseOfferingDTO>> GetStaffMyCourseAsync(int staffId, CancellationToken cancellationToken = default)
    {
        return await _context.CourseOfferings
            .AsNoTracking()
            .Where(co => co.CourseOfferingDoctors
                .Any(cod => cod.DoctorID == staffId)) 
            .Select(co => new DoctorCourseOfferingDTO
            {
                CourseOfferingID = co.CourseOfferingID,
                CourseOfferingHours = co.Course.Credit_Hours,
                CourseName = co.Course.C_Name,
                Semester = co.Semester.SemesterName
            })
            .ToListAsync(cancellationToken);
    }




    /// <summary>
    /// Get My Course Details Async.
    /// </summary>
    public  async Task<IEnumerable<CourseofferingDetailsDTO>> GetMyCourseDetailsAsync(int courseofferingId, int userId, CancellationToken cancellationToken = default)
    {
        var StudentID = await _context.Students
            .Where(s => s.UserId == userId)
            .Select(s => s.StudentId)
            .FirstOrDefaultAsync(cancellationToken);



        return await    _context.CourseDetails
            .AsNoTracking()
            .Where(cd => cd.CourseOfferingID == courseofferingId && cd.studentId == StudentID)
            .Select(cd => new CourseofferingDetailsDTO
            {
                C_Name = cd.C_Name,
                Credit_Hours = Convert.ToInt16(cd.Credit_Hours),
                ModuleId = cd.ModuleId,
                ModuleName = cd.ModuleName,
                maxDegree = cd.maxDegree,
                studentdegree = cd.studentdegree
            })
            .ToListAsync(cancellationToken);

    }










    /// <summary>
    /// Get Course Analysis Async.
    /// </summary>
    public async Task<CourseAnalysisResponseDto> GetCourseAnalysisAsync(int courseOfferingId, CancellationToken cancellationToken = default)
    {
        

        var response = new CourseAnalysisResponseDto();

        // 1. الحصول على الـ Connection من EF Core
        var connection = _context.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        // 2. جلب الـ Transaction الحالية التي فتحها الـ TransactionBehavior (إن وجدت)
        var dbTransaction = _context.Database.CurrentTransaction?.GetDbTransaction();

        // 3. تمرير الـ dbTransaction إلى Dapper في الـ transaction parameter
        using (var multi = await connection.QueryMultipleAsync(
            "dbo.sp_GetCourseAnalysis",
            new { CourseOfferingID = courseOfferingId },
            transaction: dbTransaction, // <-- التعديل الأساسي هنا
            commandType: CommandType.StoredProcedure))
        {
            response.ModuleAverages = (await multi.ReadAsync<ModuleAverageDto>()).ToList();
            response.ProgressList = (await multi.ReadAsync<ProgressDto>()).ToList();
            response.Distribution = await multi.ReadFirstOrDefaultAsync<DistributionDto>() ?? new DistributionDto();
        }

        return response;
    }











}
