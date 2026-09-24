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
  
    public const int MaxRows = 10_000;

    /// <summary>
    /// Maximum accepted upload file size in bytes (10 MiB).
    /// </summary>
    public const long MaxFileSize = 10 * 1024 * 1024;

    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="AssignCourseGradesRangeCommandHandler"/> class.

    /// </param>
    /// <param name="unitOfWork">
    /// Unit of work.
    /// </param>
     public AssignCourseGradesRangeCommandHandler(
        IUnitOfWork unitOfWork)
    { 
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Imports course grades from an Excel file.
    /// </summary>
    /// <param name="request">
    /// Course grade import request.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// True when the grades are imported successfully.
    /// </returns>
    public async Task<bool> Handle(
        AssignCourseGradesRangeCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FilePath))
            throw new ArgumentException("Excel file path is required.");

        if (!File.Exists(request.FilePath))
            throw new FileNotFoundException("Excel file was not found.");

        var fileInfo = new FileInfo(request.FilePath);

        if (fileInfo.Length > MaxFileSize)
            throw new InvalidOperationException(
                $"The maximum allowed file size is {MaxFileSize / 1024 / 1024} MB.");

        System.Text.Encoding.RegisterProvider(
            System.Text.CodePagesEncodingProvider.Instance);

        var rows = new List<GradeImportRow>();

        await using var stream = new FileStream(
            request.FilePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);

        using var reader = ExcelReaderFactory.CreateReader(stream);

        var headerRead = false;
        var academicIdIndex = -1;
        var gradeIndex = -1;

        while (reader.Read())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!headerRead)
            {
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    var header = reader.GetValue(i)?
                        .ToString()?
                        .Trim();

                    if (string.Equals(
                        header,
                        "Academic ID",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        academicIdIndex = i;
                    }

                    if (string.Equals(
                        header,
                        "Grade",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        gradeIndex = i;
                    }
                }

                if (academicIdIndex == -1)
                    throw new InvalidOperationException(
                        "The Excel file must contain an 'ACDIMICID' column.");

                if (gradeIndex == -1)
                    throw new InvalidOperationException(
                        "The Excel file must contain a 'Grade' column.");

                headerRead = true;
                continue;
            }

            if (rows.Count >= MaxRows)
                throw new InvalidOperationException(
                    $"The maximum allowed number of rows is {MaxRows}.");

            var academicId = reader.GetValue(academicIdIndex)?
                .ToString()?
                .Trim();

            if (string.IsNullOrWhiteSpace(academicId))
                throw new InvalidOperationException(
                    $"Academic ID is required at Excel row {rows.Count + 2}.");

            var gradeValue = reader.GetValue(gradeIndex);

            if (gradeValue is null ||
                gradeValue == DBNull.Value ||
                !decimal.TryParse(
                    gradeValue.ToString(),
                    out var grade))
            {
                throw new InvalidOperationException(
                    $"Invalid grade at Excel row {rows.Count + 2}.");
            }

            if (grade < 0 || grade>40)
                throw new InvalidOperationException(
                    $"Grade cannot be negative at Excel row {rows.Count + 2}.");

            rows.Add(new GradeImportRow(
                academicId,
                grade));
        }

        if (!headerRead)
            throw new InvalidOperationException(
                "The Excel file is empty.");

        if (rows.Count == 0)
            throw new InvalidOperationException(
                "The Excel file does not contain any grade rows.");

        var duplicateAcademicIds = rows
            .GroupBy(x => x.AcademicId, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateAcademicIds.Count > 0)
        {
            throw new InvalidOperationException(
                $"Duplicate Academic IDs found: " +
                $"{string.Join(", ", duplicateAcademicIds)}.");
        }

        var grades = new List<CourseGrade>(rows.Count);

        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var enrollmentId =
                await _unitOfWork.Enrollments.GetEnrollmentIdAsync(
                    row.AcademicId,
                    request.CourseOfferingID,
                    cancellationToken);

            if (!enrollmentId.HasValue)
            {
                throw new InvalidOperationException(
                    $"No enrollment was found for Academic ID " +
                    $"'{row.AcademicId}' in the specified course offering.");
            }

            grades.Add(new CourseGrade
            {
                EnrollmentID = enrollmentId.Value,
                ModuleID = request.ModelID,
                StudentDegree = row.Grade,
                CreatedBy = request.UserID
            });
        }

        await _unitOfWork.CourseGrades.AddRangeAsync(grades);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// Represents a single grade row imported from Excel.
    /// </summary>
    private sealed record GradeImportRow(
        string AcademicId,
        decimal Grade);
}


