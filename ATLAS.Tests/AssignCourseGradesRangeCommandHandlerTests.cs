using ATLAsBLL.Features.CourseGrades.Commands.CreateCourseGradesRange;
using ATLAsDCL.UnitOfWork;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
using ClosedXML.Excel;
using Moq;
using Xunit;

namespace ATLAS.Tests;

/// <summary>
/// Covers the Excel grade import at the handler level.
/// The assessment module must belong to the requested course offering, every
/// degree must fit the module's allowed range, every academic number must
/// resolve to an enrollment of the same offering, and a rejected file must
/// write nothing.
/// </summary>
public class AssignCourseGradesRangeCommandHandlerTests : IDisposable
{
    private const int CourseOfferingId = 7;
    private const int ModuleId = 21;

    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IAssessmentModuleRepository> _modules = new();
    private readonly Mock<IEnrollmentRepository> _enrollments = new();
    private readonly Mock<ICourseGradeRepository> _grades = new();
    private readonly Mock<ISemesterRepository> _semesters = new();
    private readonly List<string> _createdFiles = new();

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="AssignCourseGradesRangeCommandHandlerTests"/> class.
    /// </summary>
    public AssignCourseGradesRangeCommandHandlerTests()
    {
        _unitOfWork
            .SetupGet(work => work.AssessmentModules)
            .Returns(_modules.Object);

        _unitOfWork
            .SetupGet(work => work.Enrollments)
            .Returns(_enrollments.Object);

        _unitOfWork
            .SetupGet(work => work.CourseGrades)
            .Returns(_grades.Object);

        _unitOfWork
            .SetupGet(work => work.Semesters)
            .Returns(_semesters.Object);

        _unitOfWork
            .Setup(work => work.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Grading is open unless a test says otherwise (MED-07).
        _semesters
            .Setup(repository => repository.IsGradingClosedForOfferingAsync(It.IsAny<int>()))
            .ReturnsAsync(false);

        _grades
            .Setup(repository => repository.AddRangeAsync(
                It.IsAny<List<CourseGrade>>()))
            .Returns(Task.CompletedTask);
    }

    /// <summary>
    /// Once the semester's grading is closed the import must write nothing, and
    /// the uploaded file is still removed.
    /// </summary>
    [Fact]
    public async Task Handle_RejectsImportsWhenTheSemesterGradingIsClosed()
    {
        _semesters
            .Setup(repository => repository.IsGradingClosedForOfferingAsync(CourseOfferingId))
            .ReturnsAsync(true);

        SetupModule(CourseOfferingId);
        SetupEnrollments(("STU001", 42));
        var path = CreateWorkbook(("STU001", 10m));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateHandler().Handle(
                new AssignCourseGradesRangeCommand(
                    path,
                    CourseOfferingId,
                    ModuleId,
                    UserID: 3),
                CancellationToken.None));

        AssertNothingWasWritten();
        Assert.False(File.Exists(path));
    }

    /// <summary>
    /// A module that belongs to another course offering must be rejected.
    /// </summary>
    [Fact]
    public async Task Handle_RejectsModuleOfAnotherCourseOffering()
    {
        SetupModule(CourseOfferingId + 1);
        SetupEnrollments(("STU001", 42));
        var path = CreateWorkbook(("STU001", 10m));

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            CreateHandler().Handle(
                new AssignCourseGradesRangeCommand(
                    path,
                    CourseOfferingId,
                    ModuleId,
                    UserID: 3),
                CancellationToken.None));

        AssertNothingWasWritten();
        Assert.False(File.Exists(path));
    }

    /// <summary>
    /// A degree above the module maximum must be rejected.
    /// </summary>
    [Fact]
    public async Task Handle_RejectsDegreeAboveModuleMaxDegree()
    {
        SetupModule(CourseOfferingId, maxDegree: 20);
        SetupEnrollments(("STU001", 42));
        var path = CreateWorkbook(("STU001", 25m));

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            CreateHandler().Handle(
                new AssignCourseGradesRangeCommand(
                    path,
                    CourseOfferingId,
                    ModuleId,
                    UserID: 3),
                CancellationToken.None));

        AssertNothingWasWritten();
        Assert.False(File.Exists(path));
    }

    /// <summary>
    /// An academic number without an enrollment must be rejected.
    /// </summary>
    [Fact]
    public async Task Handle_RejectsAcademicIdWithoutEnrollment()
    {
        SetupModule(CourseOfferingId);
        SetupEnrollments(("STU001", 42));
        var path = CreateWorkbook(("GHOST", 10m));

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            CreateHandler().Handle(
                new AssignCourseGradesRangeCommand(
                    path,
                    CourseOfferingId,
                    ModuleId,
                    UserID: 3),
                CancellationToken.None));

        AssertNothingWasWritten();
        Assert.False(File.Exists(path));
    }

    /// <summary>
    /// Duplicate academic numbers inside one file must be rejected.
    /// </summary>
    [Fact]
    public async Task Handle_RejectsDuplicateAcademicIds()
    {
        SetupModule(CourseOfferingId);
        SetupEnrollments(("STU001", 42));
        var path = CreateWorkbook(("STU001", 10m), ("stu001", 11m));

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            CreateHandler().Handle(
                new AssignCourseGradesRangeCommand(
                    path,
                    CourseOfferingId,
                    ModuleId,
                    UserID: 3),
                CancellationToken.None));

        AssertNothingWasWritten();
        Assert.False(File.Exists(path));
    }

    /// <summary>
    /// A file without the expected header row must be rejected.
    /// </summary>
    [Fact]
    public async Task Handle_RejectsFileWithoutExpectedHeaders()
    {
        SetupModule(CourseOfferingId);

        var path = Path.Combine(
            Path.GetTempPath(),
            $"atlas-grade-import-{Guid.NewGuid():N}.xlsx");

        _createdFiles.Add(path);

        using (var workbook = new XLWorkbook())
        {
            var sheet = workbook.Worksheets.Add("Sheet1");
            sheet.Cell(1, 1).Value = "Code";
            sheet.Cell(1, 2).Value = "Score";
            sheet.Cell(2, 1).Value = "STU001";
            sheet.Cell(2, 2).Value = 10;
            workbook.SaveAs(path);
        }

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            CreateHandler().Handle(
                new AssignCourseGradesRangeCommand(
                    path,
                    CourseOfferingId,
                    ModuleId,
                    UserID: 3),
                CancellationToken.None));

        AssertNothingWasWritten();
        Assert.False(File.Exists(path));
    }

    /// <summary>
    /// A valid file is imported with the caller as the audit user and the
    /// uploaded file is removed.
    /// </summary>
    [Fact]
    public async Task Handle_ImportsGradesAndDeletesUploadedFile()
    {
        SetupModule(CourseOfferingId, maxDegree: 20);
        SetupEnrollments(("STU001", 42), ("STU002", 43));

        var saved = new List<CourseGrade>();

        _grades
            .Setup(repository => repository.AddRangeAsync(
                It.IsAny<List<CourseGrade>>()))
            .Callback<List<CourseGrade>>(grades => saved.AddRange(grades))
            .Returns(Task.CompletedTask);

        var path = CreateWorkbook(("STU001", 12.5m), ("STU002", 19m));

        var result = await CreateHandler().Handle(
            new AssignCourseGradesRangeCommand(
                path,
                CourseOfferingId,
                ModuleId,
                UserID: 9),
            CancellationToken.None);

        Assert.True(result);
        Assert.Equal(2, saved.Count);

        var first = saved.Single(grade => grade.EnrollmentID == 42);

        Assert.Equal(ModuleId, first.ModuleID);
        Assert.Equal(12.5m, first.StudentDegree);
        Assert.Equal(9, first.CreatedBy);

        _unitOfWork.Verify(
            work => work.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        Assert.False(File.Exists(path));
    }

    /// <summary>
    /// The validator rejects the identifiers the import cannot work with.
    /// </summary>
    [Fact]
    public void Validator_RejectsUnusableIdentifiers()
    {
        var validator = new AssignCourseGradesRangeCommandValidator();

        Assert.False(validator.Validate(
            new AssignCourseGradesRangeCommand(string.Empty, 0, 0, 0)).IsValid);

        Assert.True(validator.Validate(
            new AssignCourseGradesRangeCommand("grades.xlsx", 1, 2, 3)).IsValid);
    }


    /// <summary>
    /// Removes any upload file the handler did not delete.
    /// </summary>
    public void Dispose()
    {
        foreach (var file in _createdFiles)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
    }

    /// <summary>
    /// Registers the module that the handler resolves by id.
    /// </summary>
    private void SetupModule(
        int courseOfferingId,
        decimal maxDegree = 20)
    {
        _modules
            .Setup(repository => repository.GetByIdAsync(ModuleId))
            .ReturnsAsync(new AssessmentModule
            {
                ModuleID = ModuleId,
                CourseOfferingID = courseOfferingId,
                ModuleName = "Midterm",
                MaxDegree = maxDegree
            });
    }

    /// <summary>
    /// Registers the enrollment lookup of the request's course offering.
    /// </summary>
    private void SetupEnrollments(
        params (string AcademicId, int EnrollmentId)[] enrollments)
    {
        var map = new Dictionary<string, int>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var enrollment in enrollments)
        {
            map[enrollment.AcademicId] = enrollment.EnrollmentId;
        }

        _enrollments
            .Setup(repository => repository.GetEnrollmentIdsByAcademicNumbersAsync(
                It.IsAny<List<string>>(),
                CourseOfferingId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(map);
    }

    /// <summary>
    /// Creates an upload workbook that mirrors the layout of the grade export.
    /// </summary>
    private string CreateWorkbook(
        params (string AcademicId, object? Grade)[] rows)
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            $"atlas-grade-import-{Guid.NewGuid():N}.xlsx");

        _createdFiles.Add(path);

        using var workbook = new XLWorkbook();

        var sheet = workbook.Worksheets.Add("Course Grades");

        sheet.Cell(1, 1).Value = "Student ID";
        sheet.Cell(1, 2).Value = "Student Name";
        sheet.Cell(1, 3).Value = "Student Code";
        sheet.Cell(1, 4).Value = "Academic ID";
        sheet.Cell(1, 5).Value = "Grade";

        var row = 2;

        foreach (var (academicId, grade) in rows)
        {
            sheet.Cell(row, 4).Value = academicId;

            if (grade is string text)
            {
                sheet.Cell(row, 5).Value = text;
            }
            else if (grade is not null)
            {
                sheet.Cell(row, 5).Value = Convert.ToDouble(grade);
            }

            row++;
        }

        workbook.SaveAs(path);

        return path;
    }

    /// <summary>
    /// Creates the handler under test.
    /// </summary>
    private AssignCourseGradesRangeCommandHandler CreateHandler()
        => new(_unitOfWork.Object);

    /// <summary>
    /// Asserts that the import wrote nothing to the database.
    /// </summary>
    private void AssertNothingWasWritten()
    {
        _grades.Verify(
            repository => repository.AddRangeAsync(
                It.IsAny<List<CourseGrade>>()),
            Times.Never);

        _unitOfWork.Verify(
            work => work.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
