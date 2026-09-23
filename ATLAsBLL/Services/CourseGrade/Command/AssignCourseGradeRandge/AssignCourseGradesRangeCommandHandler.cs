using ATLAsDCL.UnitOfWork;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
using ExcelDataReader;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ATLAsBLL.Features.CourseGrades.Commands.CreateCourseGradesRange;

/// <summary>
/// Assign Course Grades Range Command Handler.
/// Imports grades from an Excel file into the target assessment module.
///
/// The file is parsed in two passes: rows are validated and the distinct
/// student academic numbers are extracted, a single batched database query
/// resolves all academic numbers to enrollment ids, and the final grade rows
/// are composed from an in-memory dictionary.
/// No database query is executed inside the row loop.
/// </summary>
public class AssignCourseGradesRangeCommandHandler
    : IRequestHandler<AssignCourseGradesRangeCommand, bool>
{
    /// <summary>
    /// Maximum number of grade rows accepted from a single import.
    /// </summary>
    public const int MaxRows = 10_000;

    /// <summary>
    /// Maximum accepted upload file size in bytes (10 MiB).
    /// Used by the handler as a defense-in-depth check and referenced by
    /// the controller so there is a single source of truth.
    /// </summary>
    public const long MaxFileSize = 10 * 1024 * 1024;

    /// <summary>
    /// _course Grade Repository.
    /// </summary>
    private readonly ICourseGradeRepository _courseGradeRepository;
    /// <summary>
    /// _unit Of Work.
    /// </summary>
    private readonly IUnitOfWork _unitOfWork;

    public AssignCourseGradesRangeCommandHandler(
        ICourseGradeRepository courseGradeRepository,
        IUnitOfWork unitOfWork)
    {
        _courseGradeRepository = courseGradeRepository;
        _unitOfWork = unitOfWork;
    }
