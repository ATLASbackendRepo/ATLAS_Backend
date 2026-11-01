using ATLASDomain.DTO.Semester;
using ATLASDomain.Entities;
using ATLAsBLL.Exceptions;
using ATLAsBLL.Features.Semesters.Commands.CreateSemester;
using ATLAsBLL.Features.Semesters.Commands.DeleteSemester;
using ATLAsBLL.Features.Semesters.Commands.UpdateSemester;
using ATLAsBLL.Features.Semesters.Queries.GetSemesterById;
using ATLAsBLL.Features.Semesters.Queries.GetSemesters;
using ATLAsDCL.Data;
using ATLAsDCL.Repositories;
using ATLAsDCL.UnitOfWork;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ATLAS.Tests
{
    public class SemesterTenantScopingTests
    {
        private static IUnitOfWork CreateUnitOfWork(AtLASDbContext context)
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

        [Fact]
        public async Task Admin_WithMissingInstitutionIdClaim_ThrowsForbiddenAccessException_OnAllOperations()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);

            var unassignedAdminMock = new Mock<ICurrentUserService>();
            unassignedAdminMock.Setup(u => u.IsSAdmin).Returns(false);
            unassignedAdminMock.Setup(u => u.InstitutionId).Returns((int?)null);

            // 1. Create
            var createHandler = new CreateSemesterCommandHandler(uow, unassignedAdminMock.Object);
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                createHandler.Handle(new CreateSemesterCommand("Fall", "2025-2026"), CancellationToken.None));

            // 2. Read by ID
            var getByIdHandler = new GetSemesterByIdQueryHandler(uow, unassignedAdminMock.Object);
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                getByIdHandler.Handle(new GetSemesterByIdQuery(1), CancellationToken.None));

            // 3. List
            var listHandler = new GetSemestersQueryHandler(uow, unassignedAdminMock.Object);
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                listHandler.Handle(new GetSemestersQuery(), CancellationToken.None));

            // 4. Update
            var updateHandler = new UpdateSemesterCommandHandler(uow, unassignedAdminMock.Object);
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                updateHandler.Handle(new UpdateSemesterCommand(1, "Fall", "2025-2026", false), CancellationToken.None));

            // 5. Delete
            var deleteHandler = new DeleteSemesterCommandHandler(uow, unassignedAdminMock.Object);
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                deleteHandler.Handle(new DeleteSemesterCommand(1), CancellationToken.None));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-99)]
        public async Task Admin_WithZeroOrNegativeInstitutionId_ThrowsForbiddenAccessException(int invalidId)
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);

            var invalidAdminMock = new Mock<ICurrentUserService>();
            invalidAdminMock.Setup(u => u.IsSAdmin).Returns(false);
            invalidAdminMock.Setup(u => u.InstitutionId).Returns(invalidId);

            var createHandler = new CreateSemesterCommandHandler(uow, invalidAdminMock.Object);
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                createHandler.Handle(new CreateSemesterCommand("Fall", "2025-2026"), CancellationToken.None));

            var getByIdHandler = new GetSemesterByIdQueryHandler(uow, invalidAdminMock.Object);
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                getByIdHandler.Handle(new GetSemesterByIdQuery(1), CancellationToken.None));

            var listHandler = new GetSemestersQueryHandler(uow, invalidAdminMock.Object);
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                listHandler.Handle(new GetSemestersQuery(), CancellationToken.None));

            var updateHandler = new UpdateSemesterCommandHandler(uow, invalidAdminMock.Object);
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                updateHandler.Handle(new UpdateSemesterCommand(1, "Fall", "2025-2026", false), CancellationToken.None));

            var deleteHandler = new DeleteSemesterCommandHandler(uow, invalidAdminMock.Object);
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                deleteHandler.Handle(new DeleteSemesterCommand(1), CancellationToken.None));
        }

        [Fact]
        public async Task SAdmin_CannotCreate_SemesterWithoutTargetInstitutionId_ThrowsArgumentException()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);

            var sAdminMock = new Mock<ICurrentUserService>();
            sAdminMock.Setup(u => u.IsSAdmin).Returns(true);
            sAdminMock.Setup(u => u.InstitutionId).Returns((int?)null);

            var handler = new CreateSemesterCommandHandler(uow, sAdminMock.Object);
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                handler.Handle(new CreateSemesterCommand("Fall", "2025-2026", InstitutionId: null), CancellationToken.None));

            Assert.Contains("Target institution ID is required", ex.Message);
        }

        [Fact]
        public async Task SAdmin_CanCreate_SemesterWithTargetInstitutionId()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);

            var sAdminMock = new Mock<ICurrentUserService>();
            sAdminMock.Setup(u => u.IsSAdmin).Returns(true);

            var handler = new CreateSemesterCommandHandler(uow, sAdminMock.Object);
            var semesterId = await handler.Handle(new CreateSemesterCommand("Fall", "2025-2026", InstitutionId: 3), CancellationToken.None);

            var created = await context.Semesters.FindAsync(semesterId);
            Assert.NotNull(created);
            Assert.Equal(3, created.InstitutionID);
        }

        [Fact]
        public async Task Admin_CreatesSemester_AssignsCallerInstitutionId()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);

            var adminMock = new Mock<ICurrentUserService>();
            adminMock.Setup(u => u.IsSAdmin).Returns(false);
            adminMock.Setup(u => u.InstitutionId).Returns(2);

            var handler = new CreateSemesterCommandHandler(uow, adminMock.Object);
            var semesterId = await handler.Handle(new CreateSemesterCommand("Spring", "2025-2026"), CancellationToken.None);

            var created = await context.Semesters.FindAsync(semesterId);
            Assert.NotNull(created);
            Assert.Equal("Spring", created.SemesterName);
            Assert.Equal("2025-2026", created.AcademicYear);
            Assert.Equal(2, created.InstitutionID);
        }

        [Fact]
        public async Task Admin_CannotCreate_DuplicateSemester_InSameInstitution_ThrowsInvalidOperationException()
        {
            await using var context = TestDbContextFactory.Create();
            var existing = new Semester { SemesterID = 1, SemesterName = "Fall", AcademicYear = "2025-2026", InstitutionID = 2 };
            context.Semesters.Add(existing);
            await context.SaveChangesAsync();

            var uow = CreateUnitOfWork(context);

            var adminMock = new Mock<ICurrentUserService>();
            adminMock.Setup(u => u.IsSAdmin).Returns(false);
            adminMock.Setup(u => u.InstitutionId).Returns(2);

            var handler = new CreateSemesterCommandHandler(uow, adminMock.Object);
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                handler.Handle(new CreateSemesterCommand("Fall", "2025-2026"), CancellationToken.None));

            Assert.Contains("already exists in this institution", ex.Message);
        }

        [Fact]
        public async Task DifferentInstitutions_CanHaveSemesters_WithSameNameAndAcademicYear()
        {
            await using var context = TestDbContextFactory.Create();
            // Inst 1 already has Fall 2025-2026
            var existingInst1 = new Semester { SemesterID = 1, SemesterName = "Fall", AcademicYear = "2025-2026", InstitutionID = 1 };
            context.Semesters.Add(existingInst1);
            await context.SaveChangesAsync();

            var uow = CreateUnitOfWork(context);

            // Admin of Inst 2 creates same name and academic year
            var admin2Mock = new Mock<ICurrentUserService>();
            admin2Mock.Setup(u => u.IsSAdmin).Returns(false);
            admin2Mock.Setup(u => u.InstitutionId).Returns(2);

            var handler = new CreateSemesterCommandHandler(uow, admin2Mock.Object);
            var semesterId = await handler.Handle(new CreateSemesterCommand("Fall", "2025-2026"), CancellationToken.None);

            var createdInst2 = await context.Semesters.FindAsync(semesterId);
            Assert.NotNull(createdInst2);
            Assert.Equal(2, createdInst2.InstitutionID);
        }

        [Fact]
        public async Task Admin_GetSemesters_ReturnsOnlyCallerInstitutionSemesters()
        {
            await using var context = TestDbContextFactory.Create();
            context.Semesters.AddRange(
                new Semester { SemesterID = 1, SemesterName = "Fall", AcademicYear = "2025-2026", InstitutionID = 1 },
                new Semester { SemesterID = 2, SemesterName = "Spring", AcademicYear = "2025-2026", InstitutionID = 2 },
                new Semester { SemesterID = 3, SemesterName = "Summer", AcademicYear = "2025-2026", InstitutionID = 2 }
            );
            await context.SaveChangesAsync();

            var uow = CreateUnitOfWork(context);

            var admin2Mock = new Mock<ICurrentUserService>();
            admin2Mock.Setup(u => u.IsSAdmin).Returns(false);
            admin2Mock.Setup(u => u.InstitutionId).Returns(2);

            var handler = new GetSemestersQueryHandler(uow, admin2Mock.Object);
            var result = await handler.Handle(new GetSemestersQuery(), CancellationToken.None);

            Assert.Equal(2, result.Count);
            Assert.All(result, s => Assert.Equal(2, s.InstitutionID));
        }

        [Fact]
        public async Task Admin_GetSemesterById_BelongingToAnotherInstitution_ThrowsForbiddenAccessException()
        {
            await using var context = TestDbContextFactory.Create();
            context.Semesters.Add(new Semester { SemesterID = 1, SemesterName = "Fall", AcademicYear = "2025-2026", InstitutionID = 1 });
            await context.SaveChangesAsync();

            var uow = CreateUnitOfWork(context);

            var admin2Mock = new Mock<ICurrentUserService>();
            admin2Mock.Setup(u => u.IsSAdmin).Returns(false);
            admin2Mock.Setup(u => u.InstitutionId).Returns(2);

            var handler = new GetSemesterByIdQueryHandler(uow, admin2Mock.Object);
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                handler.Handle(new GetSemesterByIdQuery(1), CancellationToken.None));
        }

        [Fact]
        public async Task Admin_GetSemesterById_Nonexistent_ThrowsKeyNotFoundException()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);

            var admin2Mock = new Mock<ICurrentUserService>();
            admin2Mock.Setup(u => u.IsSAdmin).Returns(false);
            admin2Mock.Setup(u => u.InstitutionId).Returns(2);

            var handler = new GetSemesterByIdQueryHandler(uow, admin2Mock.Object);
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                handler.Handle(new GetSemesterByIdQuery(9999), CancellationToken.None));
        }

        [Fact]
        public async Task Admin_CannotUpdate_SemesterBelongingToAnotherInstitution_ThrowsForbiddenAccessException()
        {
            await using var context = TestDbContextFactory.Create();
            context.Semesters.Add(new Semester { SemesterID = 1, SemesterName = "Fall", AcademicYear = "2025-2026", InstitutionID = 1 });
            await context.SaveChangesAsync();

            var uow = CreateUnitOfWork(context);

            var admin2Mock = new Mock<ICurrentUserService>();
            admin2Mock.Setup(u => u.IsSAdmin).Returns(false);
            admin2Mock.Setup(u => u.InstitutionId).Returns(2);

            var handler = new UpdateSemesterCommandHandler(uow, admin2Mock.Object);
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                handler.Handle(new UpdateSemesterCommand(1, "Fall Renamed", "2025-2026", true), CancellationToken.None));
        }

        [Fact]
        public async Task Admin_CannotUpdate_LegacyNullInstitutionSemester_ThrowsForbiddenAccessException()
        {
            await using var context = TestDbContextFactory.Create();
            context.Semesters.Add(new Semester { SemesterID = 5, SemesterName = "Legacy Fall", AcademicYear = "2024", InstitutionID = null });
            await context.SaveChangesAsync();

            var uow = CreateUnitOfWork(context);

            var admin2Mock = new Mock<ICurrentUserService>();
            admin2Mock.Setup(u => u.IsSAdmin).Returns(false);
            admin2Mock.Setup(u => u.InstitutionId).Returns(2);

            var handler = new UpdateSemesterCommandHandler(uow, admin2Mock.Object);
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                handler.Handle(new UpdateSemesterCommand(5, "Legacy Renamed", "2024", false), CancellationToken.None));
        }

        [Fact]
        public async Task Admin_CannotDelete_SemesterBelongingToAnotherInstitution_ThrowsForbiddenAccessException()
        {
            await using var context = TestDbContextFactory.Create();
            context.Semesters.Add(new Semester { SemesterID = 1, SemesterName = "Fall", AcademicYear = "2025-2026", InstitutionID = 1 });
            await context.SaveChangesAsync();

            var uow = CreateUnitOfWork(context);

            var admin2Mock = new Mock<ICurrentUserService>();
            admin2Mock.Setup(u => u.IsSAdmin).Returns(false);
            admin2Mock.Setup(u => u.InstitutionId).Returns(2);

            var handler = new DeleteSemesterCommandHandler(uow, admin2Mock.Object);
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                handler.Handle(new DeleteSemesterCommand(1), CancellationToken.None));
        }

        [Fact]
        public async Task Admin_CannotDelete_LegacyNullInstitutionSemester_ThrowsForbiddenAccessException()
        {
            await using var context = TestDbContextFactory.Create();
            context.Semesters.Add(new Semester { SemesterID = 5, SemesterName = "Legacy Fall", AcademicYear = "2024", InstitutionID = null });
            await context.SaveChangesAsync();

            var uow = CreateUnitOfWork(context);

            var admin2Mock = new Mock<ICurrentUserService>();
            admin2Mock.Setup(u => u.IsSAdmin).Returns(false);
            admin2Mock.Setup(u => u.InstitutionId).Returns(2);

            var handler = new DeleteSemesterCommandHandler(uow, admin2Mock.Object);
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                handler.Handle(new DeleteSemesterCommand(5), CancellationToken.None));
        }

        [Fact]
        public async Task Admin_CannotDelete_SemesterWithActiveCourseOfferings_ThrowsInvalidOperationException()
        {
            await using var context = TestDbContextFactory.Create();
            var semester = new Semester { SemesterID = 2, SemesterName = "Spring", AcademicYear = "2025-2026", InstitutionID = 2 };
            var course = new Course { CourseID = 101, C_Name = "CS 101", InstitutionID = 2 };
            var offering = new CourseOffering { CourseOfferingID = 10, CourseID = 101, SemesterID = 2, InstitutionID = 2 };

            context.Semesters.Add(semester);
            context.Courses.Add(course);
            context.CourseOfferings.Add(offering);
            await context.SaveChangesAsync();

            var uow = CreateUnitOfWork(context);

            var admin2Mock = new Mock<ICurrentUserService>();
            admin2Mock.Setup(u => u.IsSAdmin).Returns(false);
            admin2Mock.Setup(u => u.InstitutionId).Returns(2);

            var handler = new DeleteSemesterCommandHandler(uow, admin2Mock.Object);
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                handler.Handle(new DeleteSemesterCommand(2), CancellationToken.None));

            Assert.Contains("course offerings are associated with it", ex.Message);
        }

        [Fact]
        public async Task Admin_CanDelete_OwnEmptySemester_Succeeds()
        {
            await using var context = TestDbContextFactory.Create();
            var semester = new Semester { SemesterID = 2, SemesterName = "Empty Term", AcademicYear = "2025-2026", InstitutionID = 2 };
            context.Semesters.Add(semester);
            await context.SaveChangesAsync();

            var uow = CreateUnitOfWork(context);

            var admin2Mock = new Mock<ICurrentUserService>();
            admin2Mock.Setup(u => u.IsSAdmin).Returns(false);
            admin2Mock.Setup(u => u.InstitutionId).Returns(2);

            var handler = new DeleteSemesterCommandHandler(uow, admin2Mock.Object);
            var result = await handler.Handle(new DeleteSemesterCommand(2), CancellationToken.None);

            Assert.True(result);
            var exists = await context.Semesters.FindAsync(2);
            Assert.Null(exists);
        }

        [Fact]
        public async Task SAdmin_CanOperateAcrossInstitutions()
        {
            await using var context = TestDbContextFactory.Create();
            context.Semesters.AddRange(
                new Semester { SemesterID = 1, SemesterName = "Fall", AcademicYear = "2025-2026", InstitutionID = 1 },
                new Semester { SemesterID = 2, SemesterName = "Spring", AcademicYear = "2025-2026", InstitutionID = 2 }
            );
            await context.SaveChangesAsync();

            var uow = CreateUnitOfWork(context);

            var sAdminMock = new Mock<ICurrentUserService>();
            sAdminMock.Setup(u => u.IsSAdmin).Returns(true);

            // SAdmin List -> returns all
            var listHandler = new GetSemestersQueryHandler(uow, sAdminMock.Object);
            var list = await listHandler.Handle(new GetSemestersQuery(), CancellationToken.None);
            Assert.Equal(2, list.Count);

            // SAdmin Read Inst 1 -> succeeds
            var getByIdHandler = new GetSemesterByIdQueryHandler(uow, sAdminMock.Object);
            var sem1 = await getByIdHandler.Handle(new GetSemesterByIdQuery(1), CancellationToken.None);
            Assert.Equal("Fall", sem1.SemesterName);

            // SAdmin Update Inst 1 -> succeeds
            var updateHandler = new UpdateSemesterCommandHandler(uow, sAdminMock.Object);
            var updated = await updateHandler.Handle(new UpdateSemesterCommand(1, "Fall Updated", "2025-2026", true), CancellationToken.None);
            Assert.True(updated);

            // SAdmin Delete Inst 2 -> succeeds
            var deleteHandler = new DeleteSemesterCommandHandler(uow, sAdminMock.Object);
            var deleted = await deleteHandler.Handle(new DeleteSemesterCommand(2), CancellationToken.None);
            Assert.True(deleted);
        }
    }
}
