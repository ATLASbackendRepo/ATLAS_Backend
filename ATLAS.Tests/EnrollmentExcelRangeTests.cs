using ATLASDomain.DTO.Enrollment;
using ATLASDomain.Entities;
using ATLAsDCL.Repositories;
using ATLAsDCL.UnitOfWork;
using ATLAsBLL.Features.Enrollments.Commands.CreateEnrollmentsRange;
using ATLAsBLL.Services.Enrollment.Command.CreateEnrollmentRange;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ATLAS.Tests;

public class EnrollmentExcelRangeTests
{
    private MemoryStream CreateSampleExcelStream(List<(string AcademicId, string Name)> rows, bool includeHeaders = true)
    {
        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Enrollments");

        if (includeHeaders)
        {
            worksheet.Cell(1, 1).Value = "AcademicID";
            worksheet.Cell(1, 2).Value = "Name";
        }

        int rowNum = includeHeaders ? 2 : 1;
        foreach (var (academicId, name) in rows)
        {
            worksheet.Cell(rowNum, 1).Value = academicId;
            worksheet.Cell(rowNum, 2).Value = name;
            rowNum++;
        }

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }

    [Fact]
    public async Task CreateRangeEnrollment_ValidExcel_EnrollsMatchingStudentsAndReturnsSummary()
    {
        // Arrange
        await using var context = TestDbContextFactory.Create();

        var semester = new Semester
        {
            SemesterID = 1,
            AcademicYear = "2025-2026",
            SemesterName = "Fall"
        };
        var course = new Course { CourseID = 101, C_Name = "Computer Science 101", Credit_Hours = 3 };
        var courseOffering = new CourseOffering { CourseOfferingID = 10, CourseID = 101, SemesterID = 1, Semester = semester, Course = course };

        var user1 = new User { UserId = 1, AcademicId = "STU-001", FullName = "Alice", Password = "Password123!", Role = enUserRole.Student, IsActive = true };
        var student1 = new Student { StudentId = 1, StudentCode = "S001", AcademicId = "STU-001", UserId = 1, User = user1, IsActive = true };

        var user2 = new User { UserId = 2, AcademicId = "STU-002", FullName = "Bob", Password = "Password123!", Role = enUserRole.Student, IsActive = true };
        var student2 = new Student { StudentId = 2, StudentCode = "S002", AcademicId = "STU-002", UserId = 2, User = user2, IsActive = true };

        context.Semesters.Add(semester);
        context.Courses.Add(course);
        context.CourseOfferings.Add(courseOffering);
        context.Users.AddRange(user1, user2);
        context.Students.AddRange(student1, student2);
        await context.SaveChangesAsync();

        var unitOfWork = new UnitOfWork(
            context, null!,
            new CourseGradeRepository(context), null!,
            new CourseOfferingRepository(context), null!, null!, null!,
            new EnrollmentRepository(context),
            new SemesterRepository(context),
            new StudentRepository(context),
            new UserRepository(context), null!, null!, null!, null!, null!, null!, null!
        );

        var dto = new BulkCreateEnrollmentDTO
        {
            CourseID = 101,
            AcademicYear = "2025-2026",
            SemesterName = "Fall",
            DepartmentID = 1,
            level = 1
        };

        var stream = CreateSampleExcelStream(new()
        {
            ("STU-001", "Alice"),
            ("STU-002", "Bob")
        });

        var handler = new CreateEnrollmentsRangeCommandHandler(unitOfWork);
        var command = new CreateEnrollmentsRangeCommand(stream, "students.xlsx", dto, CreatedByUserId: 99);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalRowsProcessed);
        Assert.Equal(2, result.SuccessCount);
        Assert.Equal(0, result.AlreadyEnrolledCount);
        Assert.Equal(0, result.NotFoundCount);
        Assert.Equal(0, result.InvalidRowsCount);

        var enrollmentsInDb = context.Enrollments;
        Assert.Equal(2, enrollmentsInDb.Count());
    }

    [Fact]
    public async Task CreateRangeEnrollment_AlreadyEnrolledStudent_SkippedInSummary()
    {
        // Arrange
        await using var context = TestDbContextFactory.Create();

        var semester = new Semester { SemesterID = 1, AcademicYear = "2025-2026", SemesterName = "Fall" };
        var course = new Course { CourseID = 101, C_Name = "Computer Science 101" };
        var courseOffering = new CourseOffering { CourseOfferingID = 10, CourseID = 101, SemesterID = 1 };

        var user1 = new User { UserId = 1, AcademicId = "STU-001", FullName = "Alice", Password = "Password123!", Role = enUserRole.Student, IsActive = true };
        var student1 = new Student { StudentId = 1, StudentCode = "S001", AcademicId = "STU-001", UserId = 1, User = user1, IsActive = true };

        var user2 = new User { UserId = 2, AcademicId = "STU-002", FullName = "Bob", Password = "Password123!", Role = enUserRole.Student, IsActive = true };
        var student2 = new Student { StudentId = 2, StudentCode = "S002", AcademicId = "STU-002", UserId = 2, User = user2, IsActive = true };

        // Alice is already enrolled
        var existingEnrollment = new Enrollment { EnrollmentID = 100, StudentID = 1, CourseOfferingID = 10, EnrollmentDate = DateTime.UtcNow, CreatedByUserID = 99 };

        context.Semesters.Add(semester);
        context.Courses.Add(course);
        context.CourseOfferings.Add(courseOffering);
        context.Users.AddRange(user1, user2);
        context.Students.AddRange(student1, student2);
        context.Enrollments.Add(existingEnrollment);
        await context.SaveChangesAsync();

        var unitOfWork = new UnitOfWork(
            context, null!,
            new CourseGradeRepository(context), null!,
            new CourseOfferingRepository(context), null!, null!, null!,
            new EnrollmentRepository(context),
            new SemesterRepository(context),
            new StudentRepository(context),
            new UserRepository(context), null!, null!, null!, null!, null!, null!, null!
        );

        var dto = new BulkCreateEnrollmentDTO
        {
            CourseID = 101,
            AcademicYear = "2025-2026",
            SemesterName = "Fall"
        };

        var stream = CreateSampleExcelStream(new()
        {
            ("STU-001", "Alice"), // Already enrolled
            ("STU-002", "Bob")   // New enrollment
        });

        var handler = new CreateEnrollmentsRangeCommandHandler(unitOfWork);
        var command = new CreateEnrollmentsRangeCommand(stream, "students.xlsx", dto, CreatedByUserId: 99);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalRowsProcessed);
        Assert.Equal(1, result.SuccessCount);
        Assert.Equal(1, result.AlreadyEnrolledCount);
        Assert.Contains("STU-001", result.AlreadyEnrolledAcademicIds);
    }

    [Fact]
    public async Task CreateRangeEnrollment_MissingAcademicIdInDb_RecordedInNotFound()
    {
        // Arrange
        await using var context = TestDbContextFactory.Create();

        var semester = new Semester { SemesterID = 1, AcademicYear = "2025-2026", SemesterName = "Fall" };
        var courseOffering = new CourseOffering { CourseOfferingID = 10, CourseID = 101, SemesterID = 1 };

        context.Semesters.Add(semester);
        context.CourseOfferings.Add(courseOffering);
        await context.SaveChangesAsync();

        var unitOfWork = new UnitOfWork(
            context, null!,
            new CourseGradeRepository(context), null!,
            new CourseOfferingRepository(context), null!, null!, null!,
            new EnrollmentRepository(context),
            new SemesterRepository(context),
            new StudentRepository(context),
            new UserRepository(context), null!, null!, null!, null!, null!, null!, null!
        );

        var dto = new BulkCreateEnrollmentDTO { CourseID = 101, AcademicYear = "2025-2026", SemesterName = "Fall" };

        var stream = CreateSampleExcelStream(new()
        {
            ("UNKNOWN-999", "Ghost Student")
        });

        var handler = new CreateEnrollmentsRangeCommandHandler(unitOfWork);
        var command = new CreateEnrollmentsRangeCommand(stream, "students.xlsx", dto, CreatedByUserId: 99);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalRowsProcessed);
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(1, result.NotFoundCount);
        Assert.Contains("UNKNOWN-999", result.NotFoundAcademicIds);
    }

    [Fact]
    public async Task CreateRangeEnrollment_MissingRequiredHeaders_ThrowsInvalidDataException()
    {
        // Arrange
        await using var context = TestDbContextFactory.Create();
        var unitOfWork = new UnitOfWork(
            context, null!,
            new CourseGradeRepository(context), null!,
            new CourseOfferingRepository(context), null!, null!, null!,
            new EnrollmentRepository(context),
            new SemesterRepository(context),
            new StudentRepository(context),
            new UserRepository(context), null!, null!, null!, null!, null!, null!, null!
        );

        var dto = new BulkCreateEnrollmentDTO { CourseID = 101, AcademicYear = "2025-2026", SemesterName = "Fall" };

        // Create Excel without AcademicID header
        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sheet1");
        worksheet.Cell(1, 1).Value = "WrongHeader";
        worksheet.Cell(1, 2).Value = "Name";
        worksheet.Cell(2, 1).Value = "STU-001";
        worksheet.Cell(2, 2).Value = "Alice";

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var handler = new CreateEnrollmentsRangeCommandHandler(unitOfWork);
        var command = new CreateEnrollmentsRangeCommand(stream, "students.xlsx", dto, CreatedByUserId: 99);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidDataException>(() => handler.Handle(command, CancellationToken.None));
    }
}
