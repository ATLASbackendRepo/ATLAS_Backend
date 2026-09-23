using ATLASDomain.DTO.CourseGrade;
using ATLAsDCL.UnitOfWork;
using ClosedXML.Excel;
using MediatR;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ATLAsBLL.Features.CourseGrades.Queries.ExportCourseGrades;

/// <summary>
/// Exports the course grades of a course offering as an Excel or PDF file.
/// Student profile data and per-module degrees are resolved through the
/// course grade repository using an explicit EF Core projection with a fixed
/// number of batched queries, so no lazy navigation loading is triggered on
/// <see cref="ATLASDomain.Entities.Enrollment"/>.
/// </summary>
public class ExportCourseGradesQueryHandler
    : IRequestHandler<ExportCourseGradesQuery, CourseGradesExportResult>
{
    /// <summary>
    /// _unit Of Work.
    /// </summary>
    private readonly IUnitOfWork _unitOfWork;

    public ExportCourseGradesQueryHandler(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handle.
    /// </summary>
    /// <param name="request">request.</param>
    /// <param name="cancellationToken">cancellation Token.</param>
    public async Task<CourseGradesExportResult> Handle(
        ExportCourseGradesQuery request,
        CancellationToken cancellationToken)
    {
        if (request.CourseOfferingId <= 0)
            throw new ArgumentException(
                "Invalid course offering ID.");

        var format = request.Format.Trim().ToLowerInvariant();

        if (format is not ("excel" or "xlsx" or "pdf"))
            throw new ArgumentException(
                "Invalid export format. Use 'excel' or 'pdf'.");

        // Make sure Course Offering exists
        var courseOffering = await _unitOfWork.CourseOfferings
            .GetByIdAsync(request.CourseOfferingId);

        if (courseOffering is null)
        {
            throw new KeyNotFoundException(
                $"Course offering with ID {request.CourseOfferingId} does not exist.");
        }

        // DB -> Projection -> DTO (student name, code, academic id, degrees).
        var students = await _unitOfWork.CourseGrades
            .GetCourseGradesForExportAsync(
                request.CourseOfferingId);

        if (students.Count == 0)
        {
            throw new KeyNotFoundException(
                "No students found for this course offering.");
        }

        // Assessment modules are still needed for the table headers.
        var modules = await _unitOfWork.AssessmentModules.FindAsync(
            m => m.CourseOfferingID == request.CourseOfferingId);

        var moduleList = modules
            .OrderBy(m => m.ModuleID)
            .ToList();

        if (moduleList.Count == 0)
        {
            throw new KeyNotFoundException(
                "No assessment modules found for this course offering.");
        }

        // Generate Excel
        if (format is "excel" or "xlsx")
        {
            return GenerateExcel(
                students,
                request.CourseOfferingId);
        }

        // Generate PDF
        return GeneratePdf(
            students,
            request.CourseOfferingId);
    }
    /// <summary>
    /// Generate Excel.
    /// </summary>
    /// <param name="students">students.</param>
    /// <param name="courseOfferingId">course Offering Id.</param>
    private static CourseGradesExportResult GenerateExcel(
        List<CourseGradesExportDto> students,
        int courseOfferingId)
    {
        using var workbook = new XLWorkbook();

        var worksheet = workbook.Worksheets.Add("Course Grades");

        var column = 1;

        // Header (fixed labels are constants, module names come from users)
        worksheet.Cell(1, column++).Value = "Student ID";
        worksheet.Cell(1, column++).Value = "Student Name";
        worksheet.Cell(1, column++).Value = "Student Code";
        worksheet.Cell(1, column++).Value = "Academic ID";

        foreach (var gradeColumn in students[0].Grades)
        {
            worksheet.Cell(1, column++).Value =
                gradeColumn.ModuleName;
        }

        // Header styling
        var lastColumn = column - 1;

        var headerRange = worksheet.Range(
            1,
            1,
            1,
            lastColumn);

        headerRange.Style.Font.Bold = true;
        headerRange.Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Center;
        headerRange.Style.Alignment.Vertical =
            XLAlignmentVerticalValues.Center;

        // Data
        var row = 2;

        foreach (var student in students)
        {
            column = 1;

            // Numeric: no sanitization needed
            worksheet.Cell(row, column++).Value =
                student.StudentId;

            worksheet.Cell(row, column++).Value =
                student.StudentName;

            worksheet.Cell(row, column++).Value =
                student.StudentCode;

            worksheet.Cell(row, column++).Value =
                student.AcademicId;

            foreach (var grade in student.Grades)
            {
                if (grade.StudentDegree != null)
                {
                    worksheet.Cell(row, column).Value =
                        (double)grade.StudentDegree;
                }
                else
                {
                    worksheet.Cell(row, column).Value = "-";
                }

                column++;
            }

            row++;
        }

        worksheet.SheetView.FreezeRows(1);

        worksheet.Columns().AdjustToContents();

        foreach (var col in worksheet.Columns())
        {
            if (col.Width > 30)
                col.Width = 30;
        }

        using var stream = new MemoryStream();

        workbook.SaveAs(stream);

        return new CourseGradesExportResult
        {
            File = stream.ToArray(),
            ContentType =
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            FileName =
                $"CourseGrades_{courseOfferingId}.xlsx"
        };
    }

    /// <summary>
    /// Generate PDF.
    /// </summary>
    /// <param name="students">students.</param>
    /// <param name="courseOfferingId">course Offering Id.</param>
    private static CourseGradesExportResult GeneratePdf(
        List<CourseGradesExportDto> students,
        int courseOfferingId)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var moduleNames = students[0].Grades
            .Select(g => g.ModuleName)
            .ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(25);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header()
                    .PaddingBottom(8)
                    .Text($"Course Grades - Offering #{courseOfferingId}")
                    .FontSize(14)
                    .Bold();

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(50);   // Student ID
                        columns.RelativeColumn(3);    // Student Name
                        columns.RelativeColumn(2);    // Student Code
                        columns.RelativeColumn(2);    // Academic ID

                        foreach (var _ in moduleNames)
                            columns.RelativeColumn(1.5f);
                    });

                    // Repeats automatically on every page
                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).Text("Student ID").Bold();
                        header.Cell().Element(HeaderCell).Text("Student Name").Bold();
                        header.Cell().Element(HeaderCell).Text("Student Code").Bold();
                        header.Cell().Element(HeaderCell).Text("Academic ID").Bold();

                        foreach (var name in moduleNames)
                            header.Cell().Element(HeaderCell).Text(name).Bold();
                    });

                    foreach (var student in students)
                    {
                        table.Cell().Element(BodyCell).Text(student.StudentId.ToString());
                        table.Cell().Element(BodyCell).Text(student.StudentName);
                        table.Cell().Element(BodyCell).Text(student.StudentCode);
                        table.Cell().Element(BodyCell).Text(student.AcademicId);

                        foreach (var grade in student.Grades)
                        {
                            table.Cell().Element(BodyCell).AlignCenter().Text(
                                grade.StudentDegree is null
                                    ? "-"
                                    : grade.StudentDegree.Value.ToString("0.##"));
                        }
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });
        });

        return new CourseGradesExportResult
        {
            File = document.GeneratePdf(),
            ContentType = "application/pdf",
            FileName = $"CourseGrades_{courseOfferingId}.pdf"
        };

        static IContainer HeaderCell(IContainer c) => c
            .Background(Colors.Grey.Lighten3)
            .Border(0.5f).BorderColor(Colors.Grey.Medium)
            .Padding(4)
            .AlignCenter().AlignMiddle();

        static IContainer BodyCell(IContainer c) => c
            .Border(0.5f).BorderColor(Colors.Grey.Lighten1)
            .Padding(4);
    }
}