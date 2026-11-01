using ATLAsBLL.Services.Common;
using ATLAsBLL.Behaviors;
using ATLAsBLL.CQRS;
using ATLAsBLL.Features.Students.Commands.CreateStudentsRange;
using ATLAS.Domain.Entities;
using ATLAsDCL.UnitOfWork;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
using ATLASDomain.InterFaces.Repositories;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace ATLAS.Tests;

/// <summary>
/// Covers the student Excel import at the handler level.
/// HIGH-10: the number of rows is capped, every password is hashed before the
/// transaction opens, and the whole file costs a bounded number of round trips.
/// HIGH-08: no imported account may end up with the shared default password.
/// </summary>
public class CreateStudentsRangeCommandHandlerTests : IDisposable
{
    /// <summary>
    /// Institution name every generated row refers to.
    /// </summary>
    private const string InstitutionName = "ATLAS University";

    /// <summary>
    /// Department name every generated row refers to.
    /// </summary>
    private const string DepartmentName = "General";

    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IinstitutionRepository> _institutions = new();
    private readonly Mock<IDepartmentRepository> _departments = new();
    private readonly Mock<IStudentRepository> _students = new();
    private readonly Mock<IStudentProfileRepository> _profiles = new();
    private readonly List<string> _createdFiles = new();
    private List<Student> _writtenStudents = new();
    private List<StudentProfile> _writtenProfiles = new();

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="CreateStudentsRangeCommandHandlerTests"/> class.
    /// </summary>
    public CreateStudentsRangeCommandHandlerTests()
    {
        _unitOfWork.SetupGet(work => work.Institutions).Returns(_institutions.Object);
        _unitOfWork.SetupGet(work => work.Departments).Returns(_departments.Object);
        _unitOfWork.SetupGet(work => work.Students).Returns(_students.Object);
        _unitOfWork.SetupGet(work => work.StudentProfile).Returns(_profiles.Object);

        _institutions
            .Setup(repository => repository.GetInstitutionWithNameAsync(InstitutionName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(4);

        _departments
            .Setup(repository => repository.GetDepartmentWithnameAsync(DepartmentName, 4, It.IsAny<CancellationToken>()))
            .ReturnsAsync(9);

        _students
            .Setup(repository => repository.AddRangeAsync(It.IsAny<IEnumerable<Student>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<Student>, CancellationToken>((rows, _) => _writtenStudents = rows.ToList())
            .Returns(Task.CompletedTask);

        _students
            .Setup(repository => repository.GetStudentsByAcademicIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Student>());

        _profiles
            .Setup(repository => repository.AddRangeAsync(It.IsAny<IEnumerable<StudentProfile>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<StudentProfile>, CancellationToken>((rows, _) => _writtenProfiles = rows.ToList())
            .Returns(Task.CompletedTask);

        // The database assigns the student identities on the first SaveChanges.
        _unitOfWork
            .Setup(work => work.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1)
            .Callback(() =>
            {
                var nextId = 100;
                foreach (var student in _writtenStudents.Where(candidate => candidate.StudentId == 0))
                {
                    student.StudentId = nextId++;
                }
            });

        _unitOfWork
            .Setup(work => work.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWork
            .Setup(work => work.CommitTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWork
            .Setup(work => work.RollbackTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    /// <summary>
    /// A file above the row cap is rejected while it is parsed. Nothing is
    /// written, no transaction is opened, and the upload is removed.
    /// </summary>
    [Fact]
    public async Task Handle_RejectsFilesWithMoreRowsThanTheCap()
    {
        var path = CreateWorkbook(
            Enumerable
                .Range(1, CreateStudentsRangeCommandHandler.MaxImportRows + 1)
                .Select(index => ($"STU{index:00000}", $"Student {index}"))
                .ToArray());

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            CreateHandler().Handle(
                new CreateStudentsRangeCommand(path),
                CancellationToken.None));

        Assert.Empty(_writtenStudents);

        // The cap fails during parsing, so no password was hashed and the
        // transaction never opened.
        _unitOfWork.Verify(
            work => work.BeginTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Never);

        _students.Verify(
            repository => repository.AddRangeAsync(It.IsAny<IEnumerable<Student>>(), It.IsAny<CancellationToken>()),
            Times.Never);

        Assert.False(File.Exists(path));
    }

    /// <summary>
    /// MED-14: a row that names an institution the lookup does not know is
    /// rejected before anything is written; the lookup never creates the
    /// institution from the typo.
    /// </summary>
    [Fact]
    public async Task Handle_RejectsRowsWithAnUnknownInstitution()
    {
        _institutions
            .Setup(repository => repository.GetInstitutionWithNameAsync("Typo University", It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var path = CreateWorkbookWithNames("Typo University", DepartmentName, ("STU001", "One"));

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            CreateHandler().Handle(
                new CreateStudentsRangeCommand(path),
                CancellationToken.None));

        Assert.Empty(_writtenStudents);
        _unitOfWork.Verify(
            work => work.BeginTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Never);
        Assert.False(File.Exists(path));
    }

    /// <summary>
    /// MED-14: a row that names a department the lookup does not know for the
    /// resolved institution is rejected before anything is written.
    /// </summary>
    [Fact]
    public async Task Handle_RejectsRowsWithAnUnknownDepartment()
    {
        _departments
            .Setup(repository => repository.GetDepartmentWithnameAsync("Typo Department", 4, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var path = CreateWorkbookWithNames(InstitutionName, "Typo Department", ("STU001", "One"));

        var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
            CreateHandler().Handle(
                new CreateStudentsRangeCommand(path),
                CancellationToken.None));

        Assert.Contains("Typo Department", exception.Message);
        Assert.Empty(_writtenStudents);
        _unitOfWork.Verify(
            work => work.BeginTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Never);
        Assert.False(File.Exists(path));
    }

    /// <summary>
    /// A valid file gives every account its own password hash and is written
    /// with two SaveChanges calls instead of one per student.
    /// </summary>
    [Fact]
    public async Task Handle_WritesEveryRowWithItsOwnPasswordAndABoundedNumberOfRoundTrips()
    {
        var path = CreateWorkbook(("STU001", "One"), ("STU002", "Two"));

        var succeeded = await CreateHandler().Handle(
            new CreateStudentsRangeCommand(path),
            CancellationToken.None);

        Assert.NotNull(succeeded);
        Assert.Equal(2, succeeded.TotalImported);
        Assert.NotEmpty(succeeded.ExcelFileBytes);
        Assert.Equal(2, _writtenStudents.Count);
        Assert.Equal(2, _writtenProfiles.Count);

        // No account may keep the shared default password the import used to set.
        Assert.All(
            _writtenStudents,
            student =>
            {
                Assert.True(student.User!.MustChangePassword);
                Assert.False(BCrypt.Net.BCrypt.Verify("12345678", student.User.Password));
                Assert.Equal(4, student.InstitutionID);
                Assert.Equal(9, student.DepartmentId);
            });

        Assert.NotEqual(
            _writtenStudents[0].User!.Password,
            _writtenStudents[1].User!.Password);

        // One insert per batch, and each profile points at its own student.
        _students.Verify(
            repository => repository.AddRangeAsync(It.IsAny<IEnumerable<Student>>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _profiles.Verify(
            repository => repository.AddRangeAsync(It.IsAny<IEnumerable<StudentProfile>>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWork.Verify(
            work => work.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Exactly(2));

        _unitOfWork.Verify(
            work => work.BeginTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWork.Verify(
            work => work.CommitTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        Assert.Equal(
            _writtenStudents.Select(student => student.StudentId),
            _writtenProfiles.Select(profile => profile.StudentID));

        Assert.False(File.Exists(path));
    }

    /// <summary>
    /// A failure while inserting rolls the transaction back and still removes the
    /// uploaded file.
    /// </summary>
    [Fact]
    public async Task Handle_RollsBackTheTransaction_WhenTheInsertFails()
    {
        _students
            .Setup(repository => repository.AddRangeAsync(It.IsAny<IEnumerable<Student>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("insert failed"));

        var path = CreateWorkbook(("STU001", "One"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateHandler().Handle(
                new CreateStudentsRangeCommand(path),
                CancellationToken.None));

        _unitOfWork.Verify(
            work => work.RollbackTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWork.Verify(
            work => work.CommitTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Never);

        Assert.False(File.Exists(path));
    }

    /// <summary>
    /// The behavior does not wrap a command that manages its own transaction,
    /// which is what lets the import hash its passwords before it writes.
    /// </summary>
    [Fact]
    public async Task TransactionBehavior_LeavesSelfManagedCommandsAlone()
    {
        var behavior = new TransactionBehavior<CreateStudentsRangeCommand, StudentImportResult>(
            _unitOfWork.Object,
            NullLogger<TransactionBehavior<CreateStudentsRangeCommand, StudentImportResult>>.Instance);

        var nextCalled = false;

        var result = await behavior.Handle(
            new CreateStudentsRangeCommand("students.xlsx"),
            _ =>
            {
                nextCalled = true;
                return Task.FromResult(new StudentImportResult(0, Array.Empty<byte>(), DateTime.UtcNow));
            },
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(nextCalled);

        _unitOfWork.Verify(
            work => work.BeginTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// An ordinary command is still wrapped in a transaction.
    /// </summary>
    [Fact]
    public async Task TransactionBehavior_WrapsOrdinaryCommands()
    {
        var behavior = new TransactionBehavior<OrdinaryCommand, bool>(
            _unitOfWork.Object,
            NullLogger<TransactionBehavior<OrdinaryCommand, bool>>.Instance);

        var result = await behavior.Handle(
            new OrdinaryCommand(),
            _ => Task.FromResult(true),
            CancellationToken.None);

        Assert.True(result);

        _unitOfWork.Verify(
            work => work.BeginTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWork.Verify(
            work => work.CommitTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// When an academic ID in the Excel file already exists in the database,
    /// the handler rejects the whole file immediately without opening a transaction
    /// or inserting any rows.
    /// </summary>
    [Fact]
    public async Task Handle_RejectsFileWhenAcademicIdAlreadyExistsInDatabase()
    {
        var path = CreateWorkbook(
            ("2024001", "Existing Student"),
            ("2024002", "New Student"));

        _students
            .Setup(repository => repository.GetStudentsByAcademicIdsAsync(
                It.Is<IEnumerable<string>>(ids => ids.Contains("2024001")),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Student>
            {
                new Student { StudentId = 42, AcademicId = "2024001" }
            });

        var handler = CreateHandler();

        var exception = await Assert.ThrowsAsync<InvalidDataException>(
            () => handler.Handle(new CreateStudentsRangeCommand(path), CancellationToken.None));

        Assert.Contains("2024001", exception.Message);
        Assert.Contains("already in use", exception.Message);

        _unitOfWork.Verify(
            work => work.BeginTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Never);

        _students.Verify(
            repository => repository.AddRangeAsync(It.IsAny<IEnumerable<Student>>(), It.IsAny<CancellationToken>()),
            Times.Never);

        Assert.False(File.Exists(path));
    }

    /// <summary>
    /// BE-07 audit regression: Proves that the current implementation hashes one-time passwords
    /// and flags MustChangePassword = true, but returns only a boolean (true) without any mechanism
    /// for the caller to receive the generated temporary passwords.
    /// </summary>
    [Fact]
    public async Task Handle_GeneratesHashedPasswordsAndMarksMustChangePassword_WhileReturningOnlyBool()
    {
        var path = CreateWorkbook(
            ("2024101", "Alice Student"),
            ("2024102", "Bob Student"));

        var handler = CreateHandler();

        var result = await handler.Handle(
            new CreateStudentsRangeCommand(path),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2, result.TotalImported);
        Assert.NotEmpty(result.ExcelFileBytes);
        Assert.Equal(2, _writtenStudents.Count);

        foreach (var student in _writtenStudents)
        {
            Assert.NotNull(student.User);
            Assert.True(student.User.MustChangePassword);
            Assert.True(student.User.IsActive);
            Assert.Equal(enUserRole.Student, student.User.Role);
            Assert.StartsWith("$2", student.User.Password);
        }
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
    /// Creates the handler under test.
    /// </summary>
    /// <returns>The handler.</returns>
    private CreateStudentsRangeCommandHandler CreateHandler() => new(_unitOfWork.Object);

    /// <summary>
    /// Builds a workbook in the layout the import expects.
    /// </summary>
    /// <param name="rows">Academic number and name of every data row.</param>
    /// <returns>Path of the generated file.</returns>
    private string CreateWorkbook(params (string AcademicNumber, string FullName)[] rows)
        => CreateWorkbookWithNames(InstitutionName, DepartmentName, rows);

    /// <summary>
    /// Builds a workbook whose rows carry the given institution and department
    /// names, so a row can be rejected when a lookup does not know them.
    /// </summary>
    /// <param name="institutionName">Institution name written into every row.</param>
    /// <param name="departmentName">Department name written into every row.</param>
    /// <param name="rows">Academic number and name of every data row.</param>
    /// <returns>Path of the generated file.</returns>
    private string CreateWorkbookWithNames(
        string institutionName,
        string departmentName,
        params (string AcademicNumber, string FullName)[] rows)
    {
        var path = Path.Combine(Path.GetTempPath(), $"atlas-student-import-{Guid.NewGuid():N}.xlsx");
        _createdFiles.Add(path);

        using (var workbook = new XLWorkbook())
        {
            var sheet = workbook.Worksheets.Add("Students");

            sheet.Cell(1, 1).Value = "Institution";
            sheet.Cell(1, 2).Value = "Department";
            sheet.Cell(1, 3).Value = "FullName";
            sheet.Cell(1, 4).Value = "Level";
            sheet.Cell(1, 5).Value = "AcademicId";

            for (var index = 0; index < rows.Length; index++)
            {
                var row = index + 2;

                sheet.Cell(row, 1).Value = institutionName;
                sheet.Cell(row, 2).Value = departmentName;
                sheet.Cell(row, 3).Value = rows[index].FullName;
                sheet.Cell(row, 4).Value = "Level 1";
                sheet.Cell(row, 5).Value = rows[index].AcademicNumber;
            }

            workbook.SaveAs(path);
        }

        return path;
    }

    /// <summary>
    /// A command used to prove that the transaction behavior still wraps the
    /// commands it always did.
    /// </summary>
    private sealed record OrdinaryCommand : ICommand<bool>;
}
