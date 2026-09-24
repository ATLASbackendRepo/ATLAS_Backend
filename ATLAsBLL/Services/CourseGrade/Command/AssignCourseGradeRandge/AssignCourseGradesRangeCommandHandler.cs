using System.Globalization;
using ATLAsDCL.UnitOfWork;
using ATLASDomain.Entities;
using ExcelDataReader;
using MediatR;

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
/// The module must belong to the requested course offering, every degree must
/// fit the module's allowed range, and the uploaded file is always deleted,
/// even when the import fails.
/// </summary>
public class AssignCourseGradesRangeCommandHandler
    : IRequestHandler<AssignCourseGradesRangeCommand, bool>
{
    /// <summary>
    /// Maximum number of grade rows accepted from a single file.
    /// </summary>
    public const int MaxRows = 10_000;

    /// <summary>
    /// Maximum accepted upload file size in bytes (10 MiB).
    /// </summary>
    public const long MaxFileSize = 10 * 1024 * 1024;

    /// <summary>
    /// Header of the column holding the student academic number.
    /// </summary>
    private const string AcademicIdHeader = "Academic ID";

    /// <summary>
    /// Header of the column holding the degree.
    /// </summary>
    private const string GradeHeader = "Grade";

    /// <summary>
    /// _unit Of Work.
    /// </summary>
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="AssignCourseGradesRangeCommandHandler"/> class.
    /// </summary>
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
    /// The whole import runs inside the command transaction, so a file that
    /// contains a single bad row writes nothing.
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
        try
        {
            var rows = await ReadRowsAsync(
                request.FilePath,
                cancellationToken);

            var module = await _unitOfWork.AssessmentModules
                .GetByIdAsync(request.ModelID);

            if (module is null)
            {
                throw new KeyNotFoundException(
                    $"Assessment module with ID {request.ModelID} was not found.");
            }

            if (module.CourseOfferingID != request.CourseOfferingID)
            {
                throw new InvalidDataException(
                    $"Assessment module {request.ModelID} does not belong to " +
                    $"course offering {request.CourseOfferingID}.");
            }

            foreach (var row in rows)
            {
                if (row.Grade < 0 || row.Grade > module.MaxDegree)
                {
                    throw new InvalidDataException(
                        $"Degree {row.Grade} at Excel row {row.RowNumber} is " +
                        $"outside the allowed range 0 - {module.MaxDegree}.");
                }
            }

            var academicNumbers = rows
                .Select(row => row.AcademicId)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var enrollmentIds = await _unitOfWork.Enrollments
                .GetEnrollmentIdsByAcademicNumbersAsync(
                    academicNumbers,
                    request.CourseOfferingID,
                    cancellationToken);

            var grades = new List<CourseGrade>(rows.Count);

            foreach (var row in rows)
            {
                if (!enrollmentIds.TryGetValue(
                    row.AcademicId,
                    out var enrollmentId))
                {
                    throw new InvalidDataException(
                        $"No enrollment was found for academic ID " +
                        $"'{row.AcademicId}' in the specified course offering.");
                }

                grades.Add(new CourseGrade
                {
                    EnrollmentID = enrollmentId,
                    ModuleID = request.ModelID,
                    StudentDegree = row.Grade,
                    CreatedBy = request.UserID
                });
            }

            await _unitOfWork.CourseGrades.AddRangeAsync(grades);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
        finally
        {
            DeleteUploadedFile(request.FilePath);
        }
    }


    /// <summary>
    /// Parses and validates the uploaded Excel file.
    /// </summary>
    /// <param name="filePath">
    /// Path of the uploaded temporary file.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// The validated grade rows.
    /// </returns>
    private static async Task<List<GradeImportRow>> ReadRowsAsync(
        string filePath,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Excel file path is required.");
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Excel file was not found.");
        }

        var fileInfo = new FileInfo(filePath);

        if (fileInfo.Length > MaxFileSize)
        {
            throw new InvalidDataException(
                $"The maximum allowed file size is " +
                $"{MaxFileSize / 1024 / 1024} MB.");
        }

        System.Text.Encoding.RegisterProvider(
            System.Text.CodePagesEncodingProvider.Instance);

        var rows = new List<GradeImportRow>();

        await using var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);

        using var reader = ExcelReaderFactory.CreateReader(stream);

        var headerRead = false;
        var academicIdIndex = -1;
        var gradeIndex = -1;
        var rowNumber = 0;

        while (reader.Read())
        {
            cancellationToken.ThrowIfCancellationRequested();

            rowNumber++;

            if (!headerRead)
            {
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    var header = reader.GetValue(i)?
                        .ToString()?
                        .Trim();

                    if (string.Equals(
                        header,
                        AcademicIdHeader,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        academicIdIndex = i;
                    }

                    if (string.Equals(
                        header,
                        GradeHeader,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        gradeIndex = i;
                    }
                }

                if (academicIdIndex == -1)
                {
                    throw new InvalidDataException(
                        $"The Excel file must contain an " +
                        $"'{AcademicIdHeader}' column.");
                }

                if (gradeIndex == -1)
                {
                    throw new InvalidDataException(
                        $"The Excel file must contain a " +
                        $"'{GradeHeader}' column.");
                }

                headerRead = true;
                continue;
            }

            if (rows.Count >= MaxRows)
            {
                throw new InvalidDataException(
                    $"The maximum allowed number of rows is {MaxRows}.");
            }

            var academicId = academicIdIndex < reader.FieldCount
                ? reader.GetValue(academicIdIndex)?.ToString()?.Trim()
                : null;

            if (string.IsNullOrWhiteSpace(academicId))
            {
                throw new InvalidDataException(
                    $"Academic ID is required at Excel row {rowNumber}.");
            }

            var gradeValue = gradeIndex < reader.FieldCount
                ? reader.GetValue(gradeIndex)
                : null;

            if (gradeValue is null || gradeValue == DBNull.Value)
            {
                throw new InvalidDataException(
                    $"A degree is required at Excel row {rowNumber}.");
            }

            decimal grade;

            try
            {
                grade = Convert.ToDecimal(
                    gradeValue,
                    CultureInfo.InvariantCulture);
            }
            catch (Exception ex) when (
                ex is FormatException or
                InvalidCastException or
                OverflowException)
            {
                throw new InvalidDataException(
                    $"Invalid degree at Excel row {rowNumber}.");
            }

            rows.Add(new GradeImportRow(
                academicId,
                grade,
                rowNumber));
        }

        if (!headerRead)
        {
            throw new InvalidDataException("The Excel file is empty.");
        }

        if (rows.Count == 0)
        {
            throw new InvalidDataException(
                "The Excel file does not contain any grade rows.");
        }

        var duplicateAcademicIds = rows
            .GroupBy(
                row => row.AcademicId,
                StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateAcademicIds.Count > 0)
        {
            throw new InvalidDataException(
                $"Duplicate academic IDs found: " +
                $"{string.Join(", ", duplicateAcademicIds)}.");
        }

        return rows;
    }

    /// <summary>
    /// Deletes the uploaded temporary file, ignoring failures.
    /// </summary>
    /// <param name="filePath">
    /// Path of the uploaded temporary file.
    /// </param>
    private static void DeleteUploadedFile(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (IOException)
        {
            // The temporary upload file could not be removed now; the
            // operating system reclaims it with the temp folder.
        }
        catch (UnauthorizedAccessException)
        {
            // The temporary upload file could not be removed now; the
            // operating system reclaims it with the temp folder.
        }
    }

    /// <summary>
    /// Represents a single grade row imported from Excel.
    /// </summary>
    /// <param name="AcademicId">Student academic number.</param>
    /// <param name="Grade">Imported degree.</param>
    /// <param name="RowNumber">One based Excel row number.</param>
    private sealed record GradeImportRow(
        string AcademicId,
        decimal Grade,
        int RowNumber);


}
