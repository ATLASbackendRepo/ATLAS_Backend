using ATLASDomain.DTO.CourseGrade;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
using ATLASDomain.Views;
using ATLAsBLL.Exceptions;
using ATLAsBLL.Services.CourseGrade.Queries;
using ATLAsBLL.Services.CourseGrade.Queries.GetMySemesterRank;
using ATLAsDCL.Data;
using ATLAsDCL.Repositories;
using ATLAsDCL.UnitOfWork;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ATLAS.Tests
{
    public class RankingsTenantIsolationTests
    {
        private static IUnitOfWork CreateUnitOfWork(AtLASDbContext context, IEnrollmentRepository? enrollmentRepo = null)
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
                enrollmentRepo ?? new EnrollmentRepository(context),
                new SemesterRepository(context),
                new StudentRepository(context),
                new UserRepository(context),
                null!, null!, null!, null!, null!, null!, null!
            );
        }

        private static Mock<ICurrentUserService> CreateUserMock(bool isSAdmin, int? institutionId, enUserRole role = enUserRole.Student, int userId = 1)
        {
            var mock = new Mock<ICurrentUserService>();
            mock.Setup(u => u.IsSAdmin).Returns(isSAdmin);
            mock.Setup(u => u.InstitutionId).Returns(institutionId);
            mock.Setup(u => u.Role).Returns(role);
            mock.Setup(u => u.UserId).Returns(userId);
            return mock;
        }

        #region Phase 1: Tenant Isolation & Claims Validation

        [Fact]
        public async Task GetMyRank_MissingInstitutionClaim_ThrowsForbiddenAccessException()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);
            var userMock = CreateUserMock(isSAdmin: false, institutionId: null);

            var handler = new GetMySemesterRankQueryHandler(uow, userMock.Object);
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                handler.Handle(new GetMySemesterRankQuery(1, 1), CancellationToken.None));
        }

        [Fact]
        public async Task GetTopRanked_MissingInstitutionClaim_ThrowsForbiddenAccessException()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);
            var userMock = CreateUserMock(isSAdmin: false, institutionId: null);

            var handler = new GetTopRankedStudentsInSemesterQueryHandler(uow, userMock.Object);
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                handler.Handle(new GetTopRankedStudentsInSemesterQuery(1, 1, 10), CancellationToken.None));
        }

        [Fact]
        public async Task GetMyRank_ForeignSemester_ThrowsForbiddenAccessException()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);

            var foreignSemester = new Semester { SemesterName = "Fall", AcademicYear = "2025", InstitutionID = 99 };
            await uow.Semesters.AddAsync(foreignSemester);
            await uow.SaveChangesAsync();

            var callerMock = CreateUserMock(isSAdmin: false, institutionId: 1, userId: 10);
            var handler = new GetMySemesterRankQueryHandler(uow, callerMock.Object);

            var ex = await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                handler.Handle(new GetMySemesterRankQuery(10, foreignSemester.SemesterID), CancellationToken.None));

            Assert.Contains("not authorized", ex.Message);
        }

        [Fact]
        public async Task GetTopRanked_ForeignSemester_ThrowsForbiddenAccessException()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);

            var foreignSemester = new Semester { SemesterName = "Fall", AcademicYear = "2025", InstitutionID = 99 };
            await uow.Semesters.AddAsync(foreignSemester);
            await uow.SaveChangesAsync();

            var callerMock = CreateUserMock(isSAdmin: false, institutionId: 1);
            var handler = new GetTopRankedStudentsInSemesterQueryHandler(uow, callerMock.Object);

            var ex = await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                handler.Handle(new GetTopRankedStudentsInSemesterQuery(foreignSemester.SemesterID, 1, 5), CancellationToken.None));

            Assert.Contains("not authorized", ex.Message);
        }

        [Fact]
        public async Task TwoInstitutions_RankingsAreIsolated()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);

            var sem1 = new Semester { SemesterName = "Fall", AcademicYear = "2025", InstitutionID = 1 };
            var sem2 = new Semester { SemesterName = "Fall", AcademicYear = "2025", InstitutionID = 2 };
            await uow.Semesters.AddAsync(sem1);
            await uow.Semesters.AddAsync(sem2);
            await uow.SaveChangesAsync();

            var studentInst1 = CreateUserMock(isSAdmin: false, institutionId: 1, userId: 101);
            var studentInst2 = CreateUserMock(isSAdmin: false, institutionId: 2, userId: 201);

            var handler1 = new GetTopRankedStudentsInSemesterQueryHandler(uow, studentInst1.Object);
            var handler2 = new GetTopRankedStudentsInSemesterQueryHandler(uow, studentInst2.Object);

            // Student 1 can access sem1 but cannot access sem2
            var r1 = await handler1.Handle(new GetTopRankedStudentsInSemesterQuery(sem1.SemesterID, 1, 10), CancellationToken.None);
            Assert.NotNull(r1);

            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                handler1.Handle(new GetTopRankedStudentsInSemesterQuery(sem2.SemesterID, 1, 10), CancellationToken.None));

            // Student 2 can access sem2 but cannot access sem1
            var r2 = await handler2.Handle(new GetTopRankedStudentsInSemesterQuery(sem2.SemesterID, 1, 10), CancellationToken.None);
            Assert.NotNull(r2);

            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                handler2.Handle(new GetTopRankedStudentsInSemesterQuery(sem1.SemesterID, 1, 10), CancellationToken.None));
        }

        [Fact]
        public async Task SAdmin_CanAccessAnySemesterRankings()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);

            var foreignSemester = new Semester { SemesterName = "Fall", AcademicYear = "2025", InstitutionID = 42 };
            await uow.Semesters.AddAsync(foreignSemester);
            await uow.SaveChangesAsync();

            var sadminMock = CreateUserMock(isSAdmin: true, institutionId: null, role: enUserRole.SAdmin);
            var handler = new GetTopRankedStudentsInSemesterQueryHandler(uow, sadminMock.Object);

            var results = await handler.Handle(new GetTopRankedStudentsInSemesterQuery(foreignSemester.SemesterID, 1, 5), CancellationToken.None);
            Assert.NotNull(results);
        }

        #endregion

        #region Phase 2 & 3: Active Students, Inactive Exclusion, Ties & Dense Rank Semantics

        [Fact]
        public async Task InactiveStudent_CallingGetMyRank_ReturnsNull()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);

            var semester = new Semester { SemesterName = "Fall", AcademicYear = "2025", InstitutionID = 1 };
            var user = new User { AcademicId = "STU-INACT", FullName = "Inactive Student", Password = "hash", Role = enUserRole.Student, InstitutionID = 1 };
            var student = new Student { StudentCode = "CODE-INACT", AcademicId = "STU-INACT", AcademicLevel = 1, IsActive = false, InstitutionID = 1, User = user };

            await uow.Semesters.AddAsync(semester);
            await uow.Users.AddAsync(user);
            await uow.Students.AddAsync(student);
            await uow.SaveChangesAsync();

            var callerMock = CreateUserMock(isSAdmin: false, institutionId: 1, userId: user.UserId);
            var handler = new GetMySemesterRankQueryHandler(uow, callerMock.Object);

            var result = await handler.Handle(new GetMySemesterRankQuery(user.UserId, semester.SemesterID), CancellationToken.None);
            Assert.Null(result); // Inactive student is excluded from rankings
        }

        [Fact]
        public async Task InactiveStudents_AreExcludedFrom_GetTopRankedStudents()
        {
            await using var context = TestDbContextFactory.Create();

            var semester = new Semester { SemesterName = "Fall", AcademicYear = "2025", InstitutionID = 1 };
            var activeUser = new User { AcademicId = "ACT", FullName = "Active", Password = "hash", Role = enUserRole.Student, InstitutionID = 1 };
            var activeStudent = new Student { StudentCode = "C-ACT", AcademicId = "ACT", AcademicLevel = 1, IsActive = true, InstitutionID = 1, User = activeUser };

            await context.Semesters.AddAsync(semester);
            await context.Users.AddAsync(activeUser);
            await context.Students.AddAsync(activeStudent);
            await context.SaveChangesAsync();

            var enrollmentRepoMock = new Mock<IEnrollmentRepository>();
            enrollmentRepoMock.Setup(e => e.GetTopRankedStudentsInSemesterAsync(semester.SemesterID, 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SemesterStudentRankingView>
                {
                    new SemesterStudentRankingView { SemesterID = semester.SemesterID, StudentID = activeStudent.StudentId, AcademicLevel = 1, StudentCode = "C-ACT", FullName = "Active", TotalDegree = 95, Rank = 1 }
                });

            var uow = CreateUnitOfWork(context, enrollmentRepoMock.Object);
            var callerMock = CreateUserMock(isSAdmin: false, institutionId: 1);
            var handler = new GetTopRankedStudentsInSemesterQueryHandler(uow, callerMock.Object);

            var topList = await handler.Handle(new GetTopRankedStudentsInSemesterQuery(semester.SemesterID, 1, 10), CancellationToken.None);

            Assert.Single(topList);
            Assert.Equal(activeStudent.StudentId, topList[0].StudentId);
            Assert.Equal("Active", topList[0].FullName);
        }

        [Fact]
        public async Task TiedStudents_WithDenseRank_ReturnsAllTiedStudents_WhenRankIsWithinTopCount()
        {
            await using var context = TestDbContextFactory.Create();

            var semester = new Semester { SemesterName = "Fall", AcademicYear = "2025", InstitutionID = 1 };
            var u1 = new User { AcademicId = "TIE1", FullName = "Student Tie 1", Password = "hash", Role = enUserRole.Student, InstitutionID = 1 };
            var u2 = new User { AcademicId = "TIE2", FullName = "Student Tie 2", Password = "hash", Role = enUserRole.Student, InstitutionID = 1 };
            var s1 = new Student { StudentCode = "C-T1", AcademicId = "TIE1", AcademicLevel = 2, IsActive = true, InstitutionID = 1, User = u1 };
            var s2 = new Student { StudentCode = "C-T2", AcademicId = "TIE2", AcademicLevel = 2, IsActive = true, InstitutionID = 1, User = u2 };

            await context.Semesters.AddAsync(semester);
            await context.Users.AddRangeAsync(u1, u2);
            await context.Students.AddRangeAsync(s1, s2);
            await context.SaveChangesAsync();

            var enrollmentRepoMock = new Mock<IEnrollmentRepository>();
            // Simulating DENSE_RANK() output where both students share Rank 1
            enrollmentRepoMock.Setup(e => e.GetTopRankedStudentsInSemesterAsync(semester.SemesterID, 2, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SemesterStudentRankingView>
                {
                    new SemesterStudentRankingView { SemesterID = semester.SemesterID, StudentID = s1.StudentId, AcademicLevel = 2, StudentCode = "C-T1", FullName = "Student Tie 1", TotalDegree = 100, Rank = 1 },
                    new SemesterStudentRankingView { SemesterID = semester.SemesterID, StudentID = s2.StudentId, AcademicLevel = 2, StudentCode = "C-T2", FullName = "Student Tie 2", TotalDegree = 100, Rank = 1 }
                });

            var uow = CreateUnitOfWork(context, enrollmentRepoMock.Object);
            var callerMock = CreateUserMock(isSAdmin: false, institutionId: 1);
            var handler = new GetTopRankedStudentsInSemesterQueryHandler(uow, callerMock.Object);

            // Request top 1. Because both share Rank 1 (where Rank <= 1), both are returned per dense-rank contract.
            var topList = await handler.Handle(new GetTopRankedStudentsInSemesterQuery(semester.SemesterID, 2, 1), CancellationToken.None);

            Assert.Equal(2, topList.Count);
            Assert.All(topList, t => Assert.Equal(1, t.Rank));
        }

        [Fact]
        public async Task StudentsWithDifferentCourseCounts_RankedByRawTotalDegree_InCurrentFormula()
        {
            await using var context = TestDbContextFactory.Create();

            var semester = new Semester { SemesterName = "Fall", AcademicYear = "2025", InstitutionID = 1 };
            var u1 = new User { AcademicId = "S-MORE", FullName = "Student 5 Courses", Password = "hash", Role = enUserRole.Student, InstitutionID = 1 };
            var u2 = new User { AcademicId = "S-LESS", FullName = "Student 3 Courses", Password = "hash", Role = enUserRole.Student, InstitutionID = 1 };
            var s1 = new Student { StudentCode = "C-S1", AcademicId = "S-MORE", AcademicLevel = 1, IsActive = true, InstitutionID = 1, User = u1 };
            var s2 = new Student { StudentCode = "C-S2", AcademicId = "S-LESS", AcademicLevel = 1, IsActive = true, InstitutionID = 1, User = u2 };

            await context.Semesters.AddAsync(semester);
            await context.Users.AddRangeAsync(u1, u2);
            await context.Students.AddRangeAsync(s1, s2);
            await context.SaveChangesAsync();

            var enrollmentRepoMock = new Mock<IEnrollmentRepository>();
            // S1 took 5 courses, total = 450 (avg = 90). S2 took 3 courses, total = 295 (avg = 98.3).
            // Under current raw total degree formula, S1 is Rank 1 and S2 is Rank 2.
            enrollmentRepoMock.Setup(e => e.GetTopRankedStudentsInSemesterAsync(semester.SemesterID, 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SemesterStudentRankingView>
                {
                    new SemesterStudentRankingView { SemesterID = semester.SemesterID, StudentID = s1.StudentId, AcademicLevel = 1, StudentCode = "C-S1", FullName = "Student 5 Courses", TotalDegree = 450, Rank = 1 },
                    new SemesterStudentRankingView { SemesterID = semester.SemesterID, StudentID = s2.StudentId, AcademicLevel = 1, StudentCode = "C-S2", FullName = "Student 3 Courses", TotalDegree = 295, Rank = 2 }
                });

            var uow = CreateUnitOfWork(context, enrollmentRepoMock.Object);
            var callerMock = CreateUserMock(isSAdmin: false, institutionId: 1);
            var handler = new GetTopRankedStudentsInSemesterQueryHandler(uow, callerMock.Object);

            var topList = await handler.Handle(new GetTopRankedStudentsInSemesterQuery(semester.SemesterID, 1, 10), CancellationToken.None);

            Assert.Equal(2, topList.Count);
            Assert.Equal(s1.StudentId, topList[0].StudentId);
            Assert.Equal(1, topList[0].Rank);
            Assert.Equal(s2.StudentId, topList[1].StudentId);
            Assert.Equal(2, topList[1].Rank);
        }

        #endregion
    }
}

