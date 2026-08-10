using ATLAsDCL.UnitOfWork;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
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
        var requestedPairs = request.GradeDTO
            .Select(x => (x.EnrollmentId, x.ModuleId))
            .ToList();

        if (requestedPairs.Distinct().Count() != requestedPairs.Count)
            throw new InvalidOperationException("A grade batch cannot contain duplicate enrollment and module pairs.");

        var enrollmentIds = request.GradeDTO
            .Select(x => x.EnrollmentId)
            .Distinct()
            .ToList();

        var moduleIds = request.GradeDTO
            .Select(x => x.ModuleId)
            .Distinct()
            .ToList();

        var enrollments = await _unitOfWork.Enrollments.FindAsync(x => enrollmentIds.Contains(x.EnrollmentID));
        var enrollmentsById = enrollments.ToDictionary(x => x.EnrollmentID);
        var modules = await _unitOfWork.AssessmentModules.FindAsync(x => moduleIds.Contains(x.ModuleID));
        var modulesById = modules.ToDictionary(x => x.ModuleID);

        foreach (var item in request.GradeDTO)
        {
            if (!enrollmentsById.TryGetValue(item.EnrollmentId, out var enrollment))
                throw new KeyNotFoundException($"Enrollment with ID {item.EnrollmentId} does not exist.");

            if (!modulesById.TryGetValue(item.ModuleId, out var module))
                throw new KeyNotFoundException($"Assessment module with ID {item.ModuleId} does not exist.");

            if (enrollment.CourseOfferingID != module.CourseOfferingID)
                throw new InvalidOperationException("Every grade module must belong to the enrollment's course offering.");

            if (item.StudentDegree > module.MaxDegree)
                throw new InvalidOperationException($"Grade for module {item.ModuleId} cannot exceed its maximum degree.");
        }

        var existingGrades = await _courseGradeRepository
            .GetByEnrollmentAndModulesAsync(enrollmentIds, moduleIds);

        var duplicateStoredGrade = existingGrades
            .GroupBy(x => (x.EnrollmentID, x.ModuleID))
            .FirstOrDefault(x => x.Count() > 1);
        if (duplicateStoredGrade is not null)
            throw new InvalidOperationException("Existing duplicate course grades must be resolved before grades can be assigned.");

        var existingDictionary = existingGrades.ToDictionary(x => (x.EnrollmentID, x.ModuleID));

        var gradesToInsert = new List<CourseGrade>();

        foreach (var item in request.GradeDTO)
        {
            if (existingDictionary.TryGetValue(
                (item.EnrollmentId, item.ModuleId),
                out var existing))
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
            throw new InvalidOperationException("One or more grades were changed by another user. Refresh and try again.", ex);
        }
        catch (DbUpdateException ex) when (IsCourseGradeUniquenessViolation(ex))
        {
            // A concurrent writer inserted the same grade after the read; do not silently overwrite it.
            throw new InvalidOperationException("One or more grades were assigned concurrently. Refresh and try again.", ex);
        }

        return true;
    }

    private static bool IsCourseGradeUniquenessViolation(DbUpdateException exception) =>
        exception.InnerException?.Message.Contains("UX_CourseGrades_EnrollmentID_ModuleID", StringComparison.OrdinalIgnoreCase) == true;
}
