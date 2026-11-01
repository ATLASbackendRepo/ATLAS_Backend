using ATLASDomain.DTO.Enrollment;
using ATLASDomain.Entities;
using ATLAsBLL.Exceptions;
using ATLAsBLL.Features.Enrollments.Commands.CreateEnrollment;
using ATLAsBLL.Features.Enrollments.Commands.CreateEnrollmentsRange;
using ATLAsBLL.Features.Enrollments.Commands.DeleteEnrollment;
using ATLAsBLL.Services.Enrollment.Command.CreateEnrollmentRange;
using ATLAsBLL.Services.Enrollment.Query;
using ATLAsDCL.Repositories;
using ATLAsDCL.UnitOfWork;
using ClosedXML.Excel;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ATLAS.Tests
{
    public class EnrollmentTenantScopingTests
    {
        private static IUnitOfWork CreateUnitOfWork(ATLAsDCL.Data.AtLASDbContext context)
        {
            return new UnitOfWork(
                context,
                null!,
                new CourseGradeRepository(context),
                new CourseOfferingDoctorRepository(context),
                new CourseOfferingRepository(context),
                new CourseOfferingTARepository(context),
                new CourseRepository(context),
                new DepartmentRepository(context),
                new EnrollmentRepository(context),
                new SemesterRepository(context),
                new StudentRepository(context),
                new UserRepository(context),
                null!, null!, null!, null!, null!, null!, null!
            );
        }

        private static MemoryStream CreateExcelStream(List<(string AcademicId, string Name)> rows)
        {
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Enrollments");
            worksheet.Cell(1, 1).Value = "AcademicID";
            worksheet.Cell(1, 2).Value = "Name";

            int rowNum = 2;
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
        public async Task AdminB_CannotCreateEnrollment_WithInstitutionASemester_ThrowsKeyNotFoundException()
        {
            await using var context = TestDbContextFactory.Create();

            var semesterA = new Semester { SemesterID = 1, AcademicYear = "2025-2026", SemesterName = "Fall", InstitutionID = 1 };
            var courseA = new Course { CourseID = 101, C_Name = "Math 101", InstitutionID = 1 };
            var offeringA = new CourseOffering { CourseOfferingID = 10, CourseID = 101, SemesterID = 1, InstitutionID = 1 };
            var userA = new User { UserId = 1, AcademicId = "STU-A1", FullName = "Student A", Password = "pwd", Role = enUserRole.Student, IsActive = true, InstitutionID = 1 };
            var studentA = new Student { StudentId = 1, StudentCode = "SA01", AcademicId = "STU-A1", UserId = 1, User = userA, InstitutionID = 1, IsActive = true };

            context.Semesters.Add(semesterA);
            context.Courses.Add(courseA);
            context.CourseOfferings.Add(offeringA);
            context.Users.Add(userA);
            context.Students.Add(studentA);
            await context.SaveChangesAsync();

            var uow = CreateUnitOfWork(context);

            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(u => u.IsSAdmin).Returns(false);
            currentUserMock.Setup(u => u.InstitutionId).Returns(2);

            var handler = new CreateEnrollmentCommandHandler(uow, currentUserMock.Object);
            var dto = new BulkCreateEnrollmentDTO
            {
                CourseID = 101,
                AcademicYear = "2025-2026",
                SemesterName = "Fall",
                DepartmentID = 1,
                level = 1
            };

            var command = new CreateEnrollmentCommand("STU-A1", dto, CreatedByUserId: 20);

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Contains("was not found", ex.Message);
        }

        [Fact]
        public async Task AdminB_CannotEnroll_StudentBelongingToInstitutionA_ThrowsKeyNotFoundException()
        {
            await using var context = TestDbContextFactory.Create();

            var semesterB = new Semester { SemesterID = 2, AcademicYear = "2025-2026", SemesterName = "Fall", InstitutionID = 2 };
            var courseB = new Course { CourseID = 201, C_Name = "CS 201", InstitutionID = 2 };
            var offeringB = new CourseOffering { CourseOfferingID = 20, CourseID = 201, SemesterID = 2, InstitutionID = 2 };

            var userA = new User { UserId = 1, AcademicId = "STU-A1", FullName = "Student A", Password = "pwd", Role = enUserRole.Student, IsActive = true, InstitutionID = 1 };
            var studentA = new Student { StudentId = 1, StudentCode = "SA01", AcademicId = "STU-A1", UserId = 1, User = userA, InstitutionID = 1, IsActive = true };

            context.Semesters.Add(semesterB);
            context.Courses.Add(courseB);
            context.CourseOfferings.Add(offeringB);
            context.Users.Add(userA);
            context.Students.Add(studentA);
            await context.SaveChangesAsync();

            var uow = CreateUnitOfWork(context);

            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(u => u.IsSAdmin).Returns(false);
            currentUserMock.Setup(u => u.InstitutionId).Returns(2);

            var handler = new CreateEnrollmentCommandHandler(uow, currentUserMock.Object);
            var dto = new BulkCreateEnrollmentDTO
            {
                CourseID = 201,
                AcademicYear = "2025-2026",
                SemesterName = "Fall",
                DepartmentID = 1,
                level = 1
            };

            var command = new CreateEnrollmentCommand("STU-A1", dto, CreatedByUserId: 20);

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Contains("Student with academic ID 'STU-A1' was not found", ex.Message);
        }

        [Fact]
        public async Task AdminB_CannotDelete_EnrollmentBelongingToInstitutionA_ThrowsUnauthorizedAccessException()
        {
            await using var context = TestDbContextFactory.Create();

            var semesterA = new Semester { SemesterID = 1, AcademicYear = "2025-2026", SemesterName = "Fall", InstitutionID = 1 };
            var courseA = new Course { CourseID = 101, C_Name = "Math 101", InstitutionID = 1 };
            var offeringA = new CourseOffering { CourseOfferingID = 10, CourseID = 101, SemesterID = 1, InstitutionID = 1 };
            var userA = new User { UserId = 1, AcademicId = "STU-A1", FullName = "Student A", Password = "pwd", Role = enUserRole.Student, IsActive = true, InstitutionID = 1 };
            var studentA = new Student { StudentId = 1, StudentCode = "SA01", AcademicId = "STU-A1", UserId = 1, User = userA, InstitutionID = 1, IsActive = true };
            var enrollmentA = new Enrollment
            {
                EnrollmentID = 100,
                StudentID = 1,
                CourseOfferingID = 10,
                EnrollmentDate = DateTime.UtcNow,
                CreatedByUserID = 1
            };

            context.Semesters.Add(semesterA);
            context.Courses.Add(courseA);
            context.CourseOfferings.Add(offeringA);
            context.Users.Add(userA);
            context.Students.Add(studentA);
            context.Enrollments.Add(enrollmentA);
            await context.SaveChangesAsync();

            var uow = CreateUnitOfWork(context);

            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(u => u.IsSAdmin).Returns(false);
            currentUserMock.Setup(u => u.InstitutionId).Returns(2);

            var handler = new DeleteEnrollmentCommandHandler(uow, currentUserMock.Object);
            var command = new DeleteEnrollmentCommand(100);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task AdminB_Overview_ContainsOnlyInstitutionBOfferings()
        {
            await using var context = TestDbContextFactory.Create();

            var courseA = new Course { CourseID = 101, C_Name = "Course Alpha", InstitutionID = 1 };
            var offeringA = new CourseOffering { CourseOfferingID = 10, CourseID = 101, InstitutionID = 1 };
            context.Courses.Add(courseA);
            context.CourseOfferings.Add(offeringA);

            var courseB = new Course { CourseID = 201, C_Name = "Course Beta", InstitutionID = 2 };
            var offeringB = new CourseOffering { CourseOfferingID = 20, CourseID = 201, InstitutionID = 2 };
            context.Courses.Add(courseB);
            context.CourseOfferings.Add(offeringB);

            await context.SaveChangesAsync();

            var uow = CreateUnitOfWork(context);

            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(u => u.IsSAdmin).Returns(false);
            currentUserMock.Setup(u => u.InstitutionId).Returns(2);

            var handler = new GetAllEnrollmetHandler(uow, currentUserMock.Object);
            var result = await handler.Handle(new GetEnrollment(), CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(1, result.TotalCount);
            Assert.Single(result.Items);
            Assert.Equal(20, result.Items[0].CourseOfferingID);
            Assert.Equal("Course Beta", result.Items[0].CourseName);
        }

        [Fact]
        public async Task BulkEnrollment_FiltersOutCrossInstitutionStudents_ReportsAsNotFound()
        {
            await using var context = TestDbContextFactory.Create();

            var semesterB = new Semester { SemesterID = 2, AcademicYear = "2025-2026", SemesterName = "Fall", InstitutionID = 2 };
            var courseB = new Course { CourseID = 201, C_Name = "CS 201", InstitutionID = 2 };
            var offeringB = new CourseOffering { CourseOfferingID = 20, CourseID = 201, SemesterID = 2, InstitutionID = 2 };

            var userB = new User { UserId = 2, AcademicId = "STU-B1", FullName = "Bob Inst B", Password = "pwd", Role = enUserRole.Student, IsActive = true, InstitutionID = 2 };
            var studentB = new Student { StudentId = 2, StudentCode = "SB01", AcademicId = "STU-B1", UserId = 2, User = userB, InstitutionID = 2, IsActive = true };

            var userA = new User { UserId = 1, AcademicId = "STU-A1", FullName = "Alice Inst A", Password = "pwd", Role = enUserRole.Student, IsActive = true, InstitutionID = 1 };
            var studentA = new Student { StudentId = 1, StudentCode = "SA01", AcademicId = "STU-A1", UserId = 1, User = userA, InstitutionID = 1, IsActive = true };

            context.Semesters.Add(semesterB);
            context.Courses.Add(courseB);
            context.CourseOfferings.Add(offeringB);
            context.Users.AddRange(userB, userA);
            context.Students.AddRange(studentB, studentA);
            await context.SaveChangesAsync();

            var uow = CreateUnitOfWork(context);

            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(u => u.IsSAdmin).Returns(false);
            currentUserMock.Setup(u => u.InstitutionId).Returns(2);

            var stream = CreateExcelStream(new()
            {
                ("STU-B1", "Bob"),
                ("STU-A1", "Alice")
            });

            var dto = new BulkCreateEnrollmentDTO
            {
                CourseID = 201,
                AcademicYear = "2025-2026",
                SemesterName = "Fall",
                DepartmentID = 1,
                level = 1
            };

            var handler = new CreateEnrollmentsRangeCommandHandler(uow, currentUserMock.Object);
            var result = await handler.Handle(new CreateEnrollmentsRangeCommand(stream, "students.xlsx", dto, CreatedByUserId: 50), CancellationToken.None);

            Assert.Equal(1, result.SuccessCount);
            Assert.Equal(1, result.NotFoundCount);
            Assert.Contains("STU-A1", result.NotFoundAcademicIds);

            var enrollments = await uow.Enrollments.GetCourseEnrollmentsAsync(20);
            Assert.Single(enrollments);
            Assert.Contains(enrollments, e => e.StudentID == 2);
        }

        [Fact]
        public async Task ValidSameInstitutionEnrollment_Succeeds()
        {
            await using var context = TestDbContextFactory.Create();

            var semesterB = new Semester { SemesterID = 2, AcademicYear = "2025-2026", SemesterName = "Fall", InstitutionID = 2 };
            var courseB = new Course { CourseID = 201, C_Name = "CS 201", InstitutionID = 2 };
            var offeringB = new CourseOffering { CourseOfferingID = 20, CourseID = 201, SemesterID = 2, InstitutionID = 2 };
            var userB = new User { UserId = 2, AcademicId = "STU-B1", FullName = "Student B", Password = "pwd", Role = enUserRole.Student, IsActive = true, InstitutionID = 2 };
            var studentB = new Student { StudentId = 2, StudentCode = "SB01", AcademicId = "STU-B1", UserId = 2, User = userB, InstitutionID = 2, IsActive = true };

            context.Semesters.Add(semesterB);
            context.Courses.Add(courseB);
            context.CourseOfferings.Add(offeringB);
            context.Users.Add(userB);
            context.Students.Add(studentB);
            await context.SaveChangesAsync();

            var uow = CreateUnitOfWork(context);

            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(u => u.IsSAdmin).Returns(false);
            currentUserMock.Setup(u => u.InstitutionId).Returns(2);

            var handler = new CreateEnrollmentCommandHandler(uow, currentUserMock.Object);
            var dto = new BulkCreateEnrollmentDTO
            {
                CourseID = 201,
                AcademicYear = "2025-2026",
                SemesterName = "Fall",
                DepartmentID = 1,
                level = 1
            };

            var result = await handler.Handle(new CreateEnrollmentCommand("STU-B1", dto, CreatedByUserId: 50), CancellationToken.None);
            Assert.True(result);

            var duplicateResult = await handler.Handle(new CreateEnrollmentCommand("STU-B1", dto, CreatedByUserId: 50), CancellationToken.None);
            Assert.False(duplicateResult);
        }

        [Fact]
        public async Task SAdmin_CannotCrossEnroll_StudentA_Into_OfferingB_ThrowsArgumentException()
        {
            await using var context = TestDbContextFactory.Create();

            var semesterB = new Semester { SemesterID = 2, AcademicYear = "2025-2026", SemesterName = "Fall", InstitutionID = 2 };
            var courseB = new Course { CourseID = 201, C_Name = "CS 201", InstitutionID = 2 };
            var offeringB = new CourseOffering { CourseOfferingID = 20, CourseID = 201, SemesterID = 2, InstitutionID = 2 };

            var userA = new User { UserId = 1, AcademicId = "STU-A1", FullName = "Student A", Password = "pwd", Role = enUserRole.Student, IsActive = true, InstitutionID = 1 };
            var studentA = new Student { StudentId = 1, StudentCode = "SA01", AcademicId = "STU-A1", UserId = 1, User = userA, InstitutionID = 1, IsActive = true };

            context.Semesters.Add(semesterB);
            context.Courses.Add(courseB);
            context.CourseOfferings.Add(offeringB);
            context.Users.Add(userA);
            context.Students.Add(studentA);
            await context.SaveChangesAsync();

            var uow = CreateUnitOfWork(context);

            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(u => u.IsSAdmin).Returns(true);

            var handler = new CreateEnrollmentCommandHandler(uow, currentUserMock.Object);
            var dto = new BulkCreateEnrollmentDTO
            {
                CourseID = 201,
                AcademicYear = "2025-2026",
                SemesterName = "Fall",
                DepartmentID = 1,
                level = 1
            };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(new CreateEnrollmentCommand("STU-A1", dto, CreatedByUserId: 1), CancellationToken.None));
            Assert.Contains("different institutions", ex.Message);
        }

        [Fact]
        public async Task SAdmin_Overview_ReturnsAllInstitutions()
        {
            await using var context = TestDbContextFactory.Create();

            var courseA = new Course { CourseID = 101, C_Name = "Course Alpha", InstitutionID = 1 };
            var offeringA = new CourseOffering { CourseOfferingID = 10, CourseID = 101, InstitutionID = 1 };
            var courseB = new Course { CourseID = 201, C_Name = "Course Beta", InstitutionID = 2 };
            var offeringB = new CourseOffering { CourseOfferingID = 20, CourseID = 201, InstitutionID = 2 };

            context.Courses.AddRange(courseA, courseB);
            context.CourseOfferings.AddRange(offeringA, offeringB);
            await context.SaveChangesAsync();

            var uow = CreateUnitOfWork(context);

            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(u => u.IsSAdmin).Returns(true);

            var handler = new GetAllEnrollmetHandler(uow, currentUserMock.Object);
            var result = await handler.Handle(new GetEnrollment(), CancellationToken.None);

            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.Items.Count);
        }

        [Fact]
        public async Task Admin_WithMissingInstitutionIdClaim_ThrowsForbiddenAccessException_OnAllEndpoints()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);

            var unassignedAdminMock = new Mock<ICurrentUserService>();
            unassignedAdminMock.Setup(u => u.IsSAdmin).Returns(false);
            unassignedAdminMock.Setup(u => u.InstitutionId).Returns((int?)null);

            var dto = new BulkCreateEnrollmentDTO
            {
                CourseID = 101,
                AcademicYear = "2025-2026",
                SemesterName = "Fall",
                DepartmentID = 1,
                level = 1
            };

            // 1. Single Enrollment
            var singleHandler = new CreateEnrollmentCommandHandler(uow, unassignedAdminMock.Object);
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                singleHandler.Handle(new CreateEnrollmentCommand("STU-1", dto, 10), CancellationToken.None));

            // 2. Bulk Enrollment
            var stream = CreateExcelStream(new() { ("STU-1", "Student") });
            var bulkHandler = new CreateEnrollmentsRangeCommandHandler(uow, unassignedAdminMock.Object);
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                bulkHandler.Handle(new CreateEnrollmentsRangeCommand(stream, "students.xlsx", dto, 10), CancellationToken.None));

            // 3. Delete Enrollment
            var deleteHandler = new DeleteEnrollmentCommandHandler(uow, unassignedAdminMock.Object);
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                deleteHandler.Handle(new DeleteEnrollmentCommand(100), CancellationToken.None));

            // 4. Overview
            var overviewHandler = new GetAllEnrollmetHandler(uow, unassignedAdminMock.Object);
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                overviewHandler.Handle(new GetEnrollment(), CancellationToken.None));
        }

        [Fact]
        public async Task Admin_CannotDelete_EnrollmentWithLegacyNullOfferingInstitution_ThrowsUnauthorizedAccessException()
        {
            await using var context = TestDbContextFactory.Create();

            // Legacy offering with null InstitutionID
            var course = new Course { CourseID = 500, C_Name = "Legacy Course", InstitutionID = 1 };
            var offering = new CourseOffering { CourseOfferingID = 50, CourseID = 500, InstitutionID = null };
            var user = new User { UserId = 50, AcademicId = "STU-LEGACY", FullName = "Legacy User", Password = "pwd", Role = enUserRole.Student, IsActive = true, InstitutionID = 1 };
            var student = new Student { StudentId = 50, StudentCode = "SLEG", AcademicId = "STU-LEGACY", UserId = 50, User = user, InstitutionID = 1, IsActive = true };
            var enrollment = new Enrollment { EnrollmentID = 500, StudentID = 50, CourseOfferingID = 50, EnrollmentDate = DateTime.UtcNow, CreatedByUserID = 1 };

            context.Courses.Add(course);
            context.CourseOfferings.Add(offering);
            context.Users.Add(user);
            context.Students.Add(student);
            context.Enrollments.Add(enrollment);
            await context.SaveChangesAsync();

            var uow = CreateUnitOfWork(context);

            var adminMock = new Mock<ICurrentUserService>();
            adminMock.Setup(u => u.IsSAdmin).Returns(false);
            adminMock.Setup(u => u.InstitutionId).Returns(2);

            var handler = new DeleteEnrollmentCommandHandler(uow, adminMock.Object);

            // Must throw UnauthorizedAccessException because offering has null institution (does not match Admin's institution 2)
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                handler.Handle(new DeleteEnrollmentCommand(500), CancellationToken.None));
        }

        [Fact]
        public async Task SAdmin_CanDelete_EnrollmentAcrossInstitutions()
        {
            await using var context = TestDbContextFactory.Create();

            var courseA = new Course { CourseID = 101, C_Name = "Course Alpha", InstitutionID = 1 };
            var offeringA = new CourseOffering { CourseOfferingID = 10, CourseID = 101, InstitutionID = 1 };
            var userA = new User { UserId = 1, AcademicId = "STU-A1", FullName = "Student A", Password = "pwd", Role = enUserRole.Student, IsActive = true, InstitutionID = 1 };
            var studentA = new Student { StudentId = 1, StudentCode = "SA01", AcademicId = "STU-A1", UserId = 1, User = userA, InstitutionID = 1, IsActive = true };
            var enrollmentA = new Enrollment { EnrollmentID = 100, StudentID = 1, CourseOfferingID = 10, EnrollmentDate = DateTime.UtcNow, CreatedByUserID = 1 };

            context.Courses.Add(courseA);
            context.CourseOfferings.Add(offeringA);
            context.Users.Add(userA);
            context.Students.Add(studentA);
            context.Enrollments.Add(enrollmentA);
            await context.SaveChangesAsync();

            var uow = CreateUnitOfWork(context);

            var sAdminMock = new Mock<ICurrentUserService>();
            sAdminMock.Setup(u => u.IsSAdmin).Returns(true);

            var handler = new DeleteEnrollmentCommandHandler(uow, sAdminMock.Object);
            var result = await handler.Handle(new DeleteEnrollmentCommand(100), CancellationToken.None);

            Assert.True(result);
            var exists = await context.Enrollments.FindAsync(100);
            Assert.Null(exists);
        }

        [Fact]
        public async Task SAdmin_CannotEnroll_WhenOfferingAndSemesterInstitutionsMismatch_ThrowsArgumentException()
        {
            await using var context = TestDbContextFactory.Create();

            var semesterA = new Semester { SemesterID = 1, AcademicYear = "2025-2026", SemesterName = "Fall", InstitutionID = 1 };
            var courseB = new Course { CourseID = 201, C_Name = "CS 201", InstitutionID = 2 };
            // Offering belongs to Inst 2 but points to Semester of Inst 1
            var offeringMismatch = new CourseOffering { CourseOfferingID = 20, CourseID = 201, SemesterID = 1, InstitutionID = 2 };
            var userB = new User { UserId = 2, AcademicId = "STU-B1", FullName = "Bob", Password = "pwd", Role = enUserRole.Student, IsActive = true, InstitutionID = 2 };
            var studentB = new Student { StudentId = 2, StudentCode = "SB01", AcademicId = "STU-B1", UserId = 2, User = userB, InstitutionID = 2, IsActive = true };

            context.Semesters.Add(semesterA);
            context.Courses.Add(courseB);
            context.CourseOfferings.Add(offeringMismatch);
            context.Users.Add(userB);
            context.Students.Add(studentB);
            await context.SaveChangesAsync();

            var uow = CreateUnitOfWork(context);

            var sAdminMock = new Mock<ICurrentUserService>();
            sAdminMock.Setup(u => u.IsSAdmin).Returns(true);

            var handler = new CreateEnrollmentCommandHandler(uow, sAdminMock.Object);
            var dto = new BulkCreateEnrollmentDTO
            {
                CourseID = 201,
                AcademicYear = "2025-2026",
                SemesterName = "Fall",
                DepartmentID = 1,
                level = 1
            };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                handler.Handle(new CreateEnrollmentCommand("STU-B1", dto, 1), CancellationToken.None));
            Assert.Contains("different institutions", ex.Message);
        }

        [Fact]
        public async Task BulkEnrollment_MixedBatch_TransactionalAndCorrectStatusReporting()
        {
            await using var context = TestDbContextFactory.Create();

            var semesterB = new Semester { SemesterID = 2, AcademicYear = "2025-2026", SemesterName = "Fall", InstitutionID = 2 };
            var courseB = new Course { CourseID = 201, C_Name = "CS 201", InstitutionID = 2 };
            var offeringB = new CourseOffering { CourseOfferingID = 20, CourseID = 201, SemesterID = 2, InstitutionID = 2 };

            // Inst 2: Student 1 (to be newly enrolled)
            var userB1 = new User { UserId = 21, AcademicId = "STU-B1", FullName = "Student B1", Password = "pwd", Role = enUserRole.Student, IsActive = true, InstitutionID = 2 };
            var studentB1 = new Student { StudentId = 21, StudentCode = "SB01", AcademicId = "STU-B1", UserId = 21, User = userB1, InstitutionID = 2, IsActive = true };

            // Inst 2: Student 2 (already enrolled)
            var userB2 = new User { UserId = 22, AcademicId = "STU-B2", FullName = "Student B2", Password = "pwd", Role = enUserRole.Student, IsActive = true, InstitutionID = 2 };
            var studentB2 = new Student { StudentId = 22, StudentCode = "SB02", AcademicId = "STU-B2", UserId = 22, User = userB2, InstitutionID = 2, IsActive = true };
            var existingEnrollment = new Enrollment { EnrollmentID = 220, StudentID = 22, CourseOfferingID = 20, EnrollmentDate = DateTime.UtcNow, CreatedByUserID = 1 };

            // Inst 1: Foreign student
            var userA = new User { UserId = 11, AcademicId = "STU-A1", FullName = "Student A1", Password = "pwd", Role = enUserRole.Student, IsActive = true, InstitutionID = 1 };
            var studentA = new Student { StudentId = 11, StudentCode = "SA01", AcademicId = "STU-A1", UserId = 11, User = userA, InstitutionID = 1, IsActive = true };

            context.Semesters.Add(semesterB);
            context.Courses.Add(courseB);
            context.CourseOfferings.Add(offeringB);
            context.Users.AddRange(userB1, userB2, userA);
            context.Students.AddRange(studentB1, studentB2, studentA);
            context.Enrollments.Add(existingEnrollment);
            await context.SaveChangesAsync();

            var uow = CreateUnitOfWork(context);

            var adminMock = new Mock<ICurrentUserService>();
            adminMock.Setup(u => u.IsSAdmin).Returns(false);
            adminMock.Setup(u => u.InstitutionId).Returns(2);

            // Mixed batch:
            // 1. STU-B1 (valid, new)
            // 2. STU-B2 (already enrolled)
            // 3. STU-A1 (foreign institution - must be reported as not found)
            // 4. STU-NONEXISTENT (completely non-existent)
            var stream = CreateExcelStream(new()
            {
                ("STU-B1", "Student B1"),
                ("STU-B2", "Student B2"),
                ("STU-A1", "Student A1"),
                ("STU-NONEXISTENT", "Ghost")
            });

            var dto = new BulkCreateEnrollmentDTO
            {
                CourseID = 201,
                AcademicYear = "2025-2026",
                SemesterName = "Fall",
                DepartmentID = 1,
                level = 1
            };

            var handler = new CreateEnrollmentsRangeCommandHandler(uow, adminMock.Object);
            var result = await handler.Handle(new CreateEnrollmentsRangeCommand(stream, "batch.xlsx", dto, CreatedByUserId: 99), CancellationToken.None);

            Assert.Equal(4, result.TotalRowsProcessed);
            Assert.Equal(1, result.SuccessCount);
            Assert.Equal(1, result.AlreadyEnrolledCount);
            Assert.Contains("STU-B2", result.AlreadyEnrolledAcademicIds);
            Assert.Equal(2, result.NotFoundCount);
            Assert.Contains("STU-A1", result.NotFoundAcademicIds);
            Assert.Contains("STU-NONEXISTENT", result.NotFoundAcademicIds);

            // Verify database state: only STU-B1 and STU-B2 enrolled in offering 20
            var allOfferingEnrollments = await uow.Enrollments.GetCourseEnrollmentsAsync(20);
            Assert.Equal(2, ((List<Enrollment>)allOfferingEnrollments).Count);
        }
    }
}
