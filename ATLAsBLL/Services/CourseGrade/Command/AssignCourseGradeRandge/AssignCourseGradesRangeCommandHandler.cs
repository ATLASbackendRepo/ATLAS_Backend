using ATLAsDCL.UnitOfWork;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
using ExcelDataReader;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ATLAsBLL.Features.CourseGrades.Commands.CreateCourseGradesRange;

public class AssignCourseGradesRangeCommandHandler
    : IRequestHandler<AssignCourseGradesRangeCommand, bool>
{
    private readonly ICourseGradeRepository _courseGradeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignCourseGradesRangeCommandHandler(
        ICourseGradeRepository courseGradeRepository,
        IUnitOfWork unitOfWork)
    {
        _courseGradeRepository = courseGradeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(
        AssignCourseGradesRangeCommand request,
        CancellationToken cancellationToken)
    {
        var courseOfferingExists = await _unitOfWork.CourseOfferings 
            .GetByIdAsync(request.CourseOfferingID);

        if (courseOfferingExists == null)
            throw new KeyNotFoundException($"Course offering with ID {request.CourseOfferingID} does not exist.");

       
        var gradeDtos = await ReadGradesFromExcelAsync(
            request.FilePath,
            request.CourseOfferingID,
            request.UserID,
            request.ModelID,
            cancellationToken);

        if (gradeDtos.Count == 0)
            throw new InvalidDataException("The uploaded Excel file contains no valid grade records.");

        
        var requestedPairs = gradeDtos
            .Select(x => (x.EnrollmentId, x.ModuleId))
            .ToList();

        if (requestedPairs.Distinct().Count() != requestedPairs.Count)
            throw new InvalidOperationException("The file contains duplicate grade entries for the same student and module.");

        var enrollmentIds = gradeDtos.Select(x => x.EnrollmentId).Distinct().ToList();
        var moduleIds = gradeDtos.Select(x => x.ModuleId).Distinct().ToList();

        
        var enrollments = await _unitOfWork.Enrollments.FindAsync(x => enrollmentIds.Contains(x.EnrollmentID));
        var enrollmentsById = enrollments.ToDictionary(x => x.EnrollmentID);

        var modules = await _unitOfWork.AssessmentModules.FindAsync(x => moduleIds.Contains(x.ModuleID));
        var modulesById = modules.ToDictionary(x => x.ModuleID);

        
        foreach (var item in gradeDtos)
        {
            if (!enrollmentsById.TryGetValue(item.EnrollmentId, out var enrollment))
                throw new KeyNotFoundException($"Enrollment with ID {item.EnrollmentId} does not exist.");

            if (!modulesById.TryGetValue(item.ModuleId, out var module))
                throw new KeyNotFoundException($"Assessment module with ID {item.ModuleId} does not exist.");

            if (enrollment.CourseOfferingID != module.CourseOfferingID)
                throw new InvalidOperationException($"Module {module.ModuleID} does not belong to the enrollment's course offering.");

            if (item.StudentDegree > module.MaxDegree)
                throw new InvalidOperationException($"Grade for module {item.ModuleId} exceeds maximum allowed degree ({module.MaxDegree}).");
        }

        var existingGrades = await _courseGradeRepository
            .GetByEnrollmentAndModulesAsync(enrollmentIds, moduleIds);

        var existingDictionary = existingGrades.ToDictionary(x => (x.EnrollmentID, x.ModuleID));
        var gradesToInsert = new List<CourseGrade>();

        foreach (var item in gradeDtos)
        {
            if (existingDictionary.TryGetValue((item.EnrollmentId, item.ModuleId), out var existing))
            {
                existing.StudentDegree = item.StudentDegree;
                existing.UpdatedBy = item.UserId;
            }
            else
            {
                gradesToInsert.Add(new CourseGrade
                {
                    EnrollmentID = item.EnrollmentId,
                    ModuleID = item.ModuleId,
                    StudentDegree = item.StudentDegree,
                    CreatedBy = item.UserId
                });
            }
        }

       
        try
        {
            if (gradesToInsert.Count > 0)
                await _courseGradeRepository.AddRangeAsync(gradesToInsert);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new InvalidOperationException("One or more grades were updated by another user. Please refresh and try again.", ex);
        }
        catch (DbUpdateException ex) when (IsCourseGradeUniquenessViolation(ex))
        {
            throw new InvalidOperationException("One or more grades were assigned concurrently. Please refresh and try again.", ex);
        }
        finally
        {
            DeleteExcelFile(request.FilePath);
        }

        return true;
    }

    private async Task<List<CourseGradeImportDto>> ReadGradesFromExcelAsync(
        string filePath,
        int courseOfferingId,
        int userId,
        int modelID,
        CancellationToken cancellationToken)
    {
        var resultList = new List<CourseGradeImportDto>();

        using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = ExcelReaderFactory.CreateReader(stream);

        // In-memory lookup caches to avoid redundant queries during iterations
        var enrollmentCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var moduleCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        do
        {
            bool isHeader = true;

            while (reader.Read())
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (isHeader)
                {
                    isHeader = false;
                    continue;
                }

                // Columns layout:
                // Index 0: Student Academic ID (e.g., "2024001")
                // Index 1: Student Degree (e.g., 18.5)
                var academicNumber = GetCellValue(reader, 0);
                var degreeStr = GetCellValue(reader, 1);


                if (!decimal.TryParse(degreeStr, out decimal studentDegree))
                    throw new InvalidDataException($"Invalid degree value '{degreeStr}' for student '{academicNumber}'.");

                // Look up Enrollment ID scoped to the target Course Offering
                if (!enrollmentCache.TryGetValue(academicNumber, out int enrollmentId))
                {
                    var enrollmentID = await _unitOfWork.Enrollments.GetEnrollmentIdAsync(academicNumber ,courseOfferingId, cancellationToken);

                    if (enrollmentID == null)
                        throw new InvalidDataException($"No active enrollment found for student with academic number '{academicNumber}' in course offering ID {courseOfferingId}.");

                    enrollmentId = (int)enrollmentID;
                    enrollmentCache[academicNumber] = enrollmentId;
                }

             

                resultList.Add(new CourseGradeImportDto
                {
                    EnrollmentId = enrollmentId,
                    ModuleId = modelID,
                    StudentDegree = studentDegree,
                    UserId = userId
                });
            }
        } while (reader.NextResult());

        return resultList;
    }

    private static string GetCellValue(IExcelDataReader reader, int columnIndex)
    {
        return reader.GetValue(columnIndex)?
            .ToString()?
            .Trim() ?? string.Empty;
    }

    private static void DeleteExcelFile(string filePath)
    {
        if (!File.Exists(filePath)) return;
        try { File.Delete(filePath); } catch { }
    }

    private static bool IsCourseGradeUniquenessViolation(DbUpdateException exception) =>
        exception.InnerException?.Message.Contains("UX_CourseGrades_EnrollmentID_ModuleID", StringComparison.OrdinalIgnoreCase) == true;
}

public class CourseGradeImportDto
{
    public int EnrollmentId { get; set; }
    public int ModuleId { get; set; }
    public decimal StudentDegree { get; set; }
    public int UserId { get; set; }
}