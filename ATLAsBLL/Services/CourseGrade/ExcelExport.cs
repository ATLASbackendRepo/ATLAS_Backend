using ATLAsDCL.UnitOfWork;
using ClosedXML.Excel;
using MediatR;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ATLAsBLL.Features.CourseGrades.Queries.ExportCourseGrades;

public class ExportCourseGradesQueryHandler
    : IRequestHandler<ExportCourseGradesQuery , CourseGradesExportResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public ExportCourseGradesQueryHandler(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

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

        // Get students
        var students = await _unitOfWork.Enrollments.FindAsync(
            e => e.CourseOfferingID == request.CourseOfferingId);

        var enrollmentList = students.ToList();

        if (enrollmentList.Count == 0)
        {
            throw new KeyNotFoundException(
                "No students found for this course offering.");
        }

        var enrollmentIds = enrollmentList
            .Select(e => e.EnrollmentID)
            .ToList();

        // Get modules
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

        var moduleIds = moduleList
            .Select(m => m.ModuleID)
            .ToList();

        // Get all grades in one query
        var grades = await _unitOfWork.CourseGrades.FindAsync(
            g =>
                enrollmentIds.Contains(g.EnrollmentID) &&
                moduleIds.Contains(g.ModuleID));

        var gradeDictionary = grades.ToDictionary(
            g => (g.EnrollmentID, g.ModuleID),
            g => g.StudentDegree);

        // Generate Excel
        if (format is "excel" or "xlsx")
        {
            return GenerateExcel(
                enrollmentList,
                moduleList,
                gradeDictionary,
                request.CourseOfferingId);
        }

        // Generate PDF
        return GeneratePdf(
            enrollmentList,
            moduleList,
            gradeDictionary,
            request.CourseOfferingId);
    }

    private static CourseGradesExportResult GenerateExcel(
        List<ATLASDomain.Entities.Enrollment> enrollments,
        List<ATLASDomain.Entities.AssessmentModule> modules,
        Dictionary<(int EnrollmentId, int ModuleId), decimal> grades,
        int courseOfferingId)
    {
        using var workbook = new XLWorkbook();

        var worksheet = workbook.Worksheets.Add("Course Grades");

        var column = 1;

        // Header
        worksheet.Cell(1, column++).Value = "Student ID";
        worksheet.Cell(1, column++).Value = "Student Name";
        worksheet.Cell(1, column++).Value = "Student Code";
        worksheet.Cell(1, column++).Value = "Academic ID";

        foreach (var module in modules)
        {
            worksheet.Cell(1, column++).Value =
                module.ModuleName;
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

        foreach (var enrollment in enrollments)
        {
            column = 1;

            worksheet.Cell(row, column++).Value =
                enrollment.StudentID;

            worksheet.Cell(row, column++).Value =
                enrollment.Student?.User?.FullName ?? string.Empty;

            worksheet.Cell(row, column++).Value =
                enrollment.Student?.StudentCode ?? string.Empty;

            worksheet.Cell(row, column++).Value =
                enrollment.Student?.AcademicId ?? string.Empty;

            foreach (var module in modules)
            {
                if (grades.TryGetValue(
                        (enrollment.EnrollmentID, module.ModuleID),
                        out var degree))
                {
                    worksheet.Cell(row, column).Value =
                        (double)degree;
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

    private static CourseGradesExportResult GeneratePdf(
        List<ATLASDomain.Entities.Enrollment> enrollments,
        List<ATLASDomain.Entities.AssessmentModule> modules,
        Dictionary<(int EnrollmentId, int ModuleId), decimal> grades,
        int courseOfferingId)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());

                page.Margin(20);

                page.DefaultTextStyle(
                    x => x.FontSize(8));

                page.Header()
                    .Text($"Course Grades - Offering {courseOfferingId}")
                    .Bold()
                    .FontSize(18);

                page.Content()
                    .PaddingTop(15)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1.3f);

                            foreach (var _ in modules)
                            {
                                columns.RelativeColumn(1);
                            }
                        });

                        // Header
                        table.Header(header =>
                        {
                            header.Cell()
                                .Element(HeaderCell)
                                .Text("ID");

                            header.Cell()
                                .Element(HeaderCell)
                                .Text("Student Name");

                            header.Cell()
                                .Element(HeaderCell)
                                .Text("Code");

                            header.Cell()
                                .Element(HeaderCell)
                                .Text("Academic ID");

                            foreach (var module in modules)
                            {
                                header.Cell()
                                    .Element(HeaderCell)
                                    .Text(module.ModuleName);
                            }
                        });

                        // Students
                        foreach (var enrollment in enrollments)
                        {
                            table.Cell()
                                .Element(Cell)
                                .Text(enrollment.StudentID.ToString());

                            table.Cell()
                                .Element(Cell)
                                .Text(
                                    enrollment.Student?.User?.FullName
                                    ?? string.Empty);

                            table.Cell()
                                .Element(Cell)
                                .Text(
                                    enrollment.Student?.StudentCode
                                    ?? string.Empty);

                            table.Cell()
                                .Element(Cell)
                                .Text(
                                    enrollment.Student?.AcademicId
                                    ?? string.Empty);

                            foreach (var module in modules)
                            {
                                if (grades.TryGetValue(
                                        (
                                            enrollment.EnrollmentID,
                                            module.ModuleID),
                                        out var degree))
                                {
                                    table.Cell()
                                        .Element(Cell)
                                        .Text(degree.ToString());
                                }
                                else
                                {
                                    table.Cell()
                                        .Element(Cell)
                                        .Text("-");
                                }
                            }
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.TotalPages();
                    });
            });
        });

        var pdfBytes = document.GeneratePdf();

        return new CourseGradesExportResult
        {
            File = pdfBytes,
            ContentType = "application/pdf",
            FileName =
                $"CourseGrades_{courseOfferingId}.pdf"
        };
    }

    private static IContainer HeaderCell(
        IContainer container)
    {
        return container
            .Background("#1F4E78")
            .Padding(5)
            .Border(1)
            .AlignCenter()
            .AlignMiddle()
            .DefaultTextStyle(
                x => x.FontColor("#FFFFFF").Bold());
    }

    private static IContainer Cell(
        IContainer container)
    {
        return container
            .Padding(4)
            .Border(1)
            .AlignCenter()
            .AlignMiddle();
    }
}