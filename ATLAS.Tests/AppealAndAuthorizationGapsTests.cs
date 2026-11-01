using ATLAS.Persistence.Repositories;
using ATLASDomain.DTO.Request;
using ATLASDomain.Entities;
using ATLAsBLL.Exceptions;
using ATLAsBLL.Services.CourseOfferingDoctor.Query;
using ATLAsBLL.Services.Request.Command.CreateGradeAppealRequest;
using ATLAsBLL.Services.Request.Command.UpdateGradeAppealRequest;
using ATLAsBLL.Services.Request.Qeuries;
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
    public class AppealAndAuthorizationGapsTests
    {
        private static IUnitOfWork CreateUnitOfWork(AtLASDbContext context) => new UnitOfWork(
            context,
            new AssessmentModuleRepository(context),
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
            new CourseGroupMessageRepository(context),
            new NotificationRepository(context),
            new StudentProfileRepository(context),
            new StaffProfileRepository(context),
            new CourseGroupMessageReadRepository(context),
            new GradeAppealRequestRepository(context),
            new institutionRepository(context)
        );

        private static Mock<ICurrentUserService> CreateUserMock(
            bool isSAdmin,
            int? institutionId,
            enUserRole role = enUserRole.Admin,
            int userId = 1)
        {
            var mock = new Mock<ICurrentUserService>();
            mock.Setup(u => u.IsSAdmin).Returns(isSAdmin);
            mock.Setup(u => u.InstitutionId).Returns(institutionId);
            mock.Setup(u => u.Role).Returns(role);
            mock.Setup(u => u.UserId).Returns(userId);
            return mock;
        }

        private static async Task SeedTwoInstitutionsDataAsync(AtLASDbContext context)
        {
            // Institutions
            context.Institutions.AddRange(
                new Institution { InstitutionID = 1, name = "Institution One" },
                new Institution { InstitutionID = 2, name = "Institution Two" }
            );

            // Departments
            context.Departments.AddRange(
                new Department { DepartmentId = 1, D_Name = "CS1", InstitutionID = 1 },
                new Department { DepartmentId = 2, D_Name = "CS2", InstitutionID = 2 }
            );

            // Users
            context.Users.AddRange(
                // Students
                new User { UserId = 101, FullName = "Student One", Password = "hash", Role = enUserRole.Student, AcademicId = "S101", InstitutionID = 1, DepartmentId = 1, IsActive = true },
                new User { UserId = 102, FullName = "Student Two (Foreign)", Password = "hash", Role = enUserRole.Student, AcademicId = "S102", InstitutionID = 2, DepartmentId = 2, IsActive = true },
                new User { UserId = 103, FullName = "Student Three (Not Enrolled)", Password = "hash", Role = enUserRole.Student, AcademicId = "S103", InstitutionID = 1, DepartmentId = 1, IsActive = true },

                // Doctors
                new User { UserId = 201, FullName = "Doctor One", Password = "hash", Role = enUserRole.Doctor, AcademicId = "D201", InstitutionID = 1, DepartmentId = 1, IsActive = true },
                new User { UserId = 202, FullName = "Doctor Two (Foreign)", Password = "hash", Role = enUserRole.Doctor, AcademicId = "D202", InstitutionID = 2, DepartmentId = 2, IsActive = true },
                new User { UserId = 203, FullName = "Doctor Three (Unassigned)", Password = "hash", Role = enUserRole.Doctor, AcademicId = "D203", InstitutionID = 1, DepartmentId = 1, IsActive = true },

                // Admins
                new User { UserId = 301, FullName = "Admin One", Password = "hash", Role = enUserRole.Admin, AcademicId = "A301", InstitutionID = 1, DepartmentId = 1, IsActive = true },
                new User { UserId = 302, FullName = "Admin Two (Foreign)", Password = "hash", Role = enUserRole.Admin, AcademicId = "A302", InstitutionID = 2, DepartmentId = 2, IsActive = true },

                // SAdmin
                new User { UserId = 401, FullName = "Super Admin", Password = "hash", Role = enUserRole.SAdmin, AcademicId = "SA401", InstitutionID = 1, DepartmentId = 1, IsActive = true }
            );

            // Students profiles
            context.Students.AddRange(
                new Student { StudentId = 11, UserId = 101, StudentCode = "SC11", AcademicId = "S101", InstitutionID = 1, DepartmentId = 1, AcademicLevel = 1, IsActive = true },
                new Student { StudentId = 12, UserId = 102, StudentCode = "SC12", AcademicId = "S102", InstitutionID = 2, DepartmentId = 2, AcademicLevel = 1, IsActive = true },
                new Student { StudentId = 13, UserId = 103, StudentCode = "SC13", AcademicId = "S103", InstitutionID = 1, DepartmentId = 1, AcademicLevel = 1, IsActive = true }
            );

            // Semesters
            context.Semesters.AddRange(
                new Semester { SemesterID = 1, SemesterName = "Fall", AcademicYear = "2026", InstitutionID = 1, IsGradingClosed = false },
                new Semester { SemesterID = 2, SemesterName = "Fall", AcademicYear = "2026", InstitutionID = 2, IsGradingClosed = false }
            );

            // Courses
            context.Courses.AddRange(
                new Course { CourseID = 1, C_Name = "Algorithms", Credit_Hours = 3, InstitutionID = 1 },
                new Course { CourseID = 2, C_Name = "Database", Credit_Hours = 3, InstitutionID = 2 }
            );

            // Offerings
            context.CourseOfferings.AddRange(
                new CourseOffering { CourseOfferingID = 10, CourseID = 1, SemesterID = 1, InstitutionID = 1 },
                new CourseOffering { CourseOfferingID = 20, CourseID = 2, SemesterID = 2, InstitutionID = 2 }
            );

            // Offering Doctors
            context.CourseOfferingDoctors.AddRange(
                new CourseOfferingDoctor { CourseOfferingDoctorID = 1, CourseOfferingID = 10, DoctorID = 201 },
                new CourseOfferingDoctor { CourseOfferingDoctorID = 2, CourseOfferingID = 20, DoctorID = 202 }
            );

            // Enrollments
            context.Enrollments.AddRange(
                new Enrollment { EnrollmentID = 1001, StudentID = 11, CourseOfferingID = 10 },
                new Enrollment { EnrollmentID = 1002, StudentID = 12, CourseOfferingID = 20 }
            );

            // Assessment Modules
            context.AssessmentModules.AddRange(
                new AssessmentModule { ModuleID = 501, CourseOfferingID = 10, ModuleName = "Midterm", MaxDegree = 30 },
                new AssessmentModule { ModuleID = 502, CourseOfferingID = 20, ModuleName = "Midterm", MaxDegree = 30 }
            );

            // Course Grades
            context.CourseGrades.AddRange(
                new CourseGrade { CG_ID = 601, EnrollmentID = 1001, ModuleID = 501, StudentDegree = 25, CreatedBy = 201 },
                new CourseGrade { CG_ID = 602, EnrollmentID = 1002, ModuleID = 502, StudentDegree = 22, CreatedBy = 202 }
            );

            // Grade Appeals
            context.GradeAppealRequests.AddRange(
                new GradeAppealRequest
                {
                    RequestID = 701,
                    StudentID = 11,
                    CourseOfferingID = 10,
                    AssessmentModuleID = 501,
                    DoctorUserID = 201,
                    CurrentGrade = 25,
                    ProblemDescription = "Review Question 2",
                    Status = enGradeAppealStatus.Pending,
                    SubmittedAt = DateTime.UtcNow
                },
                new GradeAppealRequest
                {
                    RequestID = 702,
                    StudentID = 12,
                    CourseOfferingID = 20,
                    AssessmentModuleID = 502,
                    DoctorUserID = 202,
                    CurrentGrade = 22,
                    ProblemDescription = "Review Question 3",
                    Status = enGradeAppealStatus.Pending,
                    SubmittedAt = DateTime.UtcNow
                },
                new GradeAppealRequest
                {
                    RequestID = 703,
                    StudentID = 11,
                    CourseOfferingID = 10,
                    AssessmentModuleID = 501,
                    DoctorUserID = 201,
                    CurrentGrade = 25,
                    ProblemDescription = "Finalized Appeal",
                    Status = enGradeAppealStatus.Approved,
                    SubmittedAt = DateTime.UtcNow,
                    ReviewedAt = DateTime.UtcNow
                }
            );

            await context.SaveChangesAsync();
        }

        #region Area 1: Institution Listing Scoping

        [Fact]
        public async Task GetAllInstitutions_SAdmin_ReturnsAllInstitutions()
        {
            await using var context = TestDbContextFactory.Create();
            await SeedTwoInstitutionsDataAsync(context);
            var uow = CreateUnitOfWork(context);
            var userMock = CreateUserMock(isSAdmin: true, institutionId: null, role: enUserRole.SAdmin);

            var handler = new GetAllInstitutionsHandler(uow, userMock.Object);
            var result = await handler.Handle(new GetAllInstitutionsQuery(), CancellationToken.None);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetAllInstitutions_Admin_ReturnsOnlyOwnInstitution()
        {
            await using var context = TestDbContextFactory.Create();
            await SeedTwoInstitutionsDataAsync(context);
            var uow = CreateUnitOfWork(context);
            var userMock = CreateUserMock(isSAdmin: false, institutionId: 1, role: enUserRole.Admin);

            var handler = new GetAllInstitutionsHandler(uow, userMock.Object);
            var result = await handler.Handle(new GetAllInstitutionsQuery(), CancellationToken.None);

            Assert.Single(result);
            Assert.Equal(1, result[0].InstitutionID);
        }

        [Fact]
        public async Task GetAllInstitutions_Admin_MissingInstitutionClaim_ThrowsForbidden()
        {
            await using var context = TestDbContextFactory.Create();
            await SeedTwoInstitutionsDataAsync(context);
            var uow = CreateUnitOfWork(context);
            var userMock = CreateUserMock(isSAdmin: false, institutionId: null, role: enUserRole.Admin);

            var handler = new GetAllInstitutionsHandler(uow, userMock.Object);
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                handler.Handle(new GetAllInstitutionsQuery(), CancellationToken.None));
        }

        [Fact]
        public async Task GetInstitutionById_Admin_OwnInstitution_Succeeds()
        {
            await using var context = TestDbContextFactory.Create();
            await SeedTwoInstitutionsDataAsync(context);
            var uow = CreateUnitOfWork(context);
            var userMock = CreateUserMock(isSAdmin: false, institutionId: 1, role: enUserRole.Admin);

            var handler = new GetInstitutionByIdHandler(uow, userMock.Object);
            var result = await handler.Handle(new GetInstitutionByIdQuery(1), CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(1, result.InstitutionID);
        }

        [Fact]
        public async Task GetInstitutionById_Admin_ForeignInstitution_ThrowsForbidden()
        {
            await using var context = TestDbContextFactory.Create();
            await SeedTwoInstitutionsDataAsync(context);
            var uow = CreateUnitOfWork(context);
            var userMock = CreateUserMock(isSAdmin: false, institutionId: 1, role: enUserRole.Admin);

            var handler = new GetInstitutionByIdHandler(uow, userMock.Object);
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                handler.Handle(new GetInstitutionByIdQuery(2), CancellationToken.None));
        }

        [Fact]
        public async Task GetInstitutionById_SAdmin_ForeignInstitution_Succeeds()
        {
            await using var context = TestDbContextFactory.Create();
            await SeedTwoInstitutionsDataAsync(context);
            var uow = CreateUnitOfWork(context);
            var userMock = CreateUserMock(isSAdmin: true, institutionId: null, role: enUserRole.SAdmin);

            var handler = new GetInstitutionByIdHandler(uow, userMock.Object);
            var result = await handler.Handle(new GetInstitutionByIdQuery(2), CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(2, result.InstitutionID);
        }

        #endregion

        #region Area 2: Course Offering DoctorIDs Scoping

        [Fact]
        public async Task GetDoctorIDs_EnrolledStudentSameInstitution_Succeeds()
        {
            await using var context = TestDbContextFactory.Create();
            await SeedTwoInstitutionsDataAsync(context);
            var uow = CreateUnitOfWork(context);
            var userMock = CreateUserMock(isSAdmin: false, institutionId: 1, role: enUserRole.Student, userId: 101);

            var handler = new GetDoctorIDQueryHandler(uow, userMock.Object);
            var result = await handler.Handle(new GetDoctorIDQuery(10), CancellationToken.None);

            Assert.Contains(201, result);
        }

        [Fact]
        public async Task GetDoctorIDs_StudentNotEnrolled_ThrowsForbidden()
        {
            await using var context = TestDbContextFactory.Create();
            await SeedTwoInstitutionsDataAsync(context);
            var uow = CreateUnitOfWork(context);
            // Student 103 belongs to Institution 1, but is not enrolled in offering 10
            var userMock = CreateUserMock(isSAdmin: false, institutionId: 1, role: enUserRole.Student, userId: 103);

            var handler = new GetDoctorIDQueryHandler(uow, userMock.Object);
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                handler.Handle(new GetDoctorIDQuery(10), CancellationToken.None));
        }

        [Fact]
        public async Task GetDoctorIDs_StudentForeignInstitution_ThrowsForbidden()
        {
            await using var context = TestDbContextFactory.Create();
            await SeedTwoInstitutionsDataAsync(context);
            var uow = CreateUnitOfWork(context);
            // Student 102 belongs to Institution 2, attempting to access offering 10 (Institution 1)
            var userMock = CreateUserMock(isSAdmin: false, institutionId: 2, role: enUserRole.Student, userId: 102);

            var handler = new GetDoctorIDQueryHandler(uow, userMock.Object);
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                handler.Handle(new GetDoctorIDQuery(10), CancellationToken.None));
        }

        [Fact]
        public async Task GetDoctorIDs_MissingInstitutionClaim_ThrowsForbidden()
        {
            await using var context = TestDbContextFactory.Create();
            await SeedTwoInstitutionsDataAsync(context);
            var uow = CreateUnitOfWork(context);
            var userMock = CreateUserMock(isSAdmin: false, institutionId: null, role: enUserRole.Student, userId: 101);

            var handler = new GetDoctorIDQueryHandler(uow, userMock.Object);
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                handler.Handle(new GetDoctorIDQuery(10), CancellationToken.None));
        }

        [Fact]
        public async Task GetDoctorIDs_SAdmin_CanViewAnyOfferingDoctors()
        {
            await using var context = TestDbContextFactory.Create();
            await SeedTwoInstitutionsDataAsync(context);
            var uow = CreateUnitOfWork(context);
            var userMock = CreateUserMock(isSAdmin: true, institutionId: null, role: enUserRole.SAdmin, userId: 401);

            var handler = new GetDoctorIDQueryHandler(uow, userMock.Object);
            var result = await handler.Handle(new GetDoctorIDQuery(20), CancellationToken.None);

            Assert.Contains(202, result);
        }

        #endregion

        #region Area 3: Student Appeal Lookup & Ownership

        [Fact]
        public async Task GetGradeAppealById_OwnerStudent_ResolvesByUserId_Succeeds()
        {
            await using var context = TestDbContextFactory.Create();
            await SeedTwoInstitutionsDataAsync(context);
            var uow = CreateUnitOfWork(context);
            // Student 101 has StudentId 11 and owns appeal 701
            var userMock = CreateUserMock(isSAdmin: false, institutionId: 1, role: enUserRole.Student, userId: 101);

            var handler = new GetGradeAppealByIdQueryHandler(uow, userMock.Object);
            var result = await handler.Handle(new GetGradeAppealByIdQuery(701, 101, null, false), CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(701, result.RequestID);
            Assert.Equal(11, result.StudentID);
        }

        [Fact]
        public async Task GetGradeAppealById_AnotherStudent_ThrowsForbidden()
        {
            await using var context = TestDbContextFactory.Create();
            await SeedTwoInstitutionsDataAsync(context);
            var uow = CreateUnitOfWork(context);
            // Student 103 (Institution 1) attempts to view Student 101's appeal 701
            var userMock = CreateUserMock(isSAdmin: false, institutionId: 1, role: enUserRole.Student, userId: 103);

            var handler = new GetGradeAppealByIdQueryHandler(uow, userMock.Object);
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                handler.Handle(new GetGradeAppealByIdQuery(701, 103, null, false), CancellationToken.None));
        }

        [Fact]
        public async Task GetGradeAppealById_CrossTenantAdmin_ThrowsForbidden()
        {
            await using var context = TestDbContextFactory.Create();
            await SeedTwoInstitutionsDataAsync(context);
            var uow = CreateUnitOfWork(context);
            // Admin 302 from Institution 2 attempts to view appeal 701 from Institution 1
            var userMock = CreateUserMock(isSAdmin: false, institutionId: 2, role: enUserRole.Admin, userId: 302);

            var handler = new GetGradeAppealByIdQueryHandler(uow, userMock.Object);
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                handler.Handle(new GetGradeAppealByIdQuery(701, 302, null, false), CancellationToken.None));
        }

        [Fact]
        public async Task GetGradeAppealById_OwnInstitutionAdmin_Succeeds()
        {
            await using var context = TestDbContextFactory.Create();
            await SeedTwoInstitutionsDataAsync(context);
            var uow = CreateUnitOfWork(context);
            // Admin 301 from Institution 1 views appeal 701 from Institution 1
            var userMock = CreateUserMock(isSAdmin: false, institutionId: 1, role: enUserRole.Admin, userId: 301);

            var handler = new GetGradeAppealByIdQueryHandler(uow, userMock.Object);
            var result = await handler.Handle(new GetGradeAppealByIdQuery(701, 301, null, false), CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(701, result.RequestID);
        }

        [Fact]
        public async Task GetGradeAppealById_AssignedDoctor_Succeeds()
        {
            await using var context = TestDbContextFactory.Create();
            await SeedTwoInstitutionsDataAsync(context);
            var uow = CreateUnitOfWork(context);
            // Doctor 201 is assigned to appeal 701
            var userMock = CreateUserMock(isSAdmin: false, institutionId: 1, role: enUserRole.Doctor, userId: 201);

            var handler = new GetGradeAppealByIdQueryHandler(uow, userMock.Object);
            var result = await handler.Handle(new GetGradeAppealByIdQuery(701, 201, null, false), CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(701, result.RequestID);
        }

        [Fact]
        public async Task GetGradeAppealById_UnassignedDoctor_ThrowsForbidden()
        {
            await using var context = TestDbContextFactory.Create();
            await SeedTwoInstitutionsDataAsync(context);
            var uow = CreateUnitOfWork(context);
            // Doctor 203 is not assigned to appeal 701
            var userMock = CreateUserMock(isSAdmin: false, institutionId: 1, role: enUserRole.Doctor, userId: 203);

            var handler = new GetGradeAppealByIdQueryHandler(uow, userMock.Object);
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                handler.Handle(new GetGradeAppealByIdQuery(701, 203, null, false), CancellationToken.None));
        }

        [Fact]
        public async Task GetGradeAppealById_MissingInstitutionClaim_ThrowsForbidden()
        {
            await using var context = TestDbContextFactory.Create();
            await SeedTwoInstitutionsDataAsync(context);
            var uow = CreateUnitOfWork(context);
            var userMock = CreateUserMock(isSAdmin: false, institutionId: null, role: enUserRole.Student, userId: 101);

            var handler = new GetGradeAppealByIdQueryHandler(uow, userMock.Object);
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                handler.Handle(new GetGradeAppealByIdQuery(701, 101, null, false), CancellationToken.None));
        }

        [Fact]
        public async Task GetGradeAppealById_SAdmin_SucceedsAcrossTenants()
        {
            await using var context = TestDbContextFactory.Create();
            await SeedTwoInstitutionsDataAsync(context);
            var uow = CreateUnitOfWork(context);
            var userMock = CreateUserMock(isSAdmin: true, institutionId: null, role: enUserRole.SAdmin, userId: 401);

            var handler = new GetGradeAppealByIdQueryHandler(uow, userMock.Object);
            var result1 = await handler.Handle(new GetGradeAppealByIdQuery(701, 401, null, true), CancellationToken.None);
            var result2 = await handler.Handle(new GetGradeAppealByIdQuery(702, 401, null, true), CancellationToken.None);

            Assert.NotNull(result1);
            Assert.NotNull(result2);
        }

        #endregion

        #region Area 4: Appeal State Transitions and Mutation Protection

        [Fact]
        public void Transitions_OpenStates_AllowedProgression()
        {
            Assert.True(GradeAppealStatusTransitions.IsAllowed(enGradeAppealStatus.Pending, enGradeAppealStatus.UnderReview));
            Assert.True(GradeAppealStatusTransitions.IsAllowed(enGradeAppealStatus.Pending, enGradeAppealStatus.Approved));
            Assert.True(GradeAppealStatusTransitions.IsAllowed(enGradeAppealStatus.Pending, enGradeAppealStatus.Rejected));
            Assert.True(GradeAppealStatusTransitions.IsAllowed(enGradeAppealStatus.Pending, enGradeAppealStatus.Closed));
            Assert.True(GradeAppealStatusTransitions.IsAllowed(enGradeAppealStatus.UnderReview, enGradeAppealStatus.Approved));
            Assert.True(GradeAppealStatusTransitions.IsAllowed(enGradeAppealStatus.UnderReview, enGradeAppealStatus.Rejected));
            Assert.True(GradeAppealStatusTransitions.IsAllowed(enGradeAppealStatus.UnderReview, enGradeAppealStatus.Closed));
            Assert.True(GradeAppealStatusTransitions.IsAllowed(enGradeAppealStatus.UnderReview, enGradeAppealStatus.UnderReview));
        }

        [Fact]
        public void Transitions_FinalStates_RejectSameStateAndAnyTransition()
        {
            // Approved is final: cannot repeat Approved or jump to any other status
            Assert.False(GradeAppealStatusTransitions.IsAllowed(enGradeAppealStatus.Approved, enGradeAppealStatus.Approved));
            Assert.False(GradeAppealStatusTransitions.IsAllowed(enGradeAppealStatus.Approved, enGradeAppealStatus.Rejected));
            Assert.False(GradeAppealStatusTransitions.IsAllowed(enGradeAppealStatus.Approved, enGradeAppealStatus.UnderReview));
            Assert.False(GradeAppealStatusTransitions.IsAllowed(enGradeAppealStatus.Approved, enGradeAppealStatus.Closed));

            // Rejected is final: cannot repeat Rejected or jump to any other status
            Assert.False(GradeAppealStatusTransitions.IsAllowed(enGradeAppealStatus.Rejected, enGradeAppealStatus.Rejected));
            Assert.False(GradeAppealStatusTransitions.IsAllowed(enGradeAppealStatus.Rejected, enGradeAppealStatus.Approved));
            Assert.False(GradeAppealStatusTransitions.IsAllowed(enGradeAppealStatus.Rejected, enGradeAppealStatus.UnderReview));
            Assert.False(GradeAppealStatusTransitions.IsAllowed(enGradeAppealStatus.Rejected, enGradeAppealStatus.Closed));

            // Closed is final: cannot repeat Closed or jump to any other status
            Assert.False(GradeAppealStatusTransitions.IsAllowed(enGradeAppealStatus.Closed, enGradeAppealStatus.Closed));
            Assert.False(GradeAppealStatusTransitions.IsAllowed(enGradeAppealStatus.Closed, enGradeAppealStatus.Approved));
        }

        [Fact]
        public async Task UpdateAppeal_ApprovedToApproved_ThrowsInvalidOperationException()
        {
            await using var context = TestDbContextFactory.Create();
            await SeedTwoInstitutionsDataAsync(context);
            var uow = CreateUnitOfWork(context);
            // Appeal 703 is already Approved
            var userMock = CreateUserMock(isSAdmin: false, institutionId: 1, role: enUserRole.Doctor, userId: 201);

            var handler = new UpdateGradeAppealRequestCommandHandler(uow, userMock.Object);
            var command = new UpdateGradeAppealRequestCommand(
                703,
                201,
                new UpdateGradeAppealRequestDto { Status = enGradeAppealStatus.Approved, DoctorResponse = "Re-approval attempt" }
            );

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task UpdateAppeal_ApprovedToRejected_ThrowsInvalidOperationException()
        {
            await using var context = TestDbContextFactory.Create();
            await SeedTwoInstitutionsDataAsync(context);
            var uow = CreateUnitOfWork(context);
            // Appeal 703 is already Approved
            var userMock = CreateUserMock(isSAdmin: false, institutionId: 1, role: enUserRole.Doctor, userId: 201);

            var handler = new UpdateGradeAppealRequestCommandHandler(uow, userMock.Object);
            var command = new UpdateGradeAppealRequestCommand(
                703,
                201,
                new UpdateGradeAppealRequestDto { Status = enGradeAppealStatus.Rejected, DoctorResponse = "Reversal attempt" }
            );

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task UpdateAppeal_UnassignedDoctor_ThrowsUnauthorizedAccessException()
        {
            await using var context = TestDbContextFactory.Create();
            await SeedTwoInstitutionsDataAsync(context);
            var uow = CreateUnitOfWork(context);
            // Doctor 203 is not assigned to appeal 701
            var userMock = CreateUserMock(isSAdmin: false, institutionId: 1, role: enUserRole.Doctor, userId: 203);

            var handler = new UpdateGradeAppealRequestCommandHandler(uow, userMock.Object);
            var command = new UpdateGradeAppealRequestCommand(
                701,
                203,
                new UpdateGradeAppealRequestDto { Status = enGradeAppealStatus.UnderReview, DoctorResponse = "Reviewing" }
            );

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task UpdateAppeal_CrossTenantDoctor_ThrowsUnauthorizedAccessException()
        {
            await using var context = TestDbContextFactory.Create();
            await SeedTwoInstitutionsDataAsync(context);
            var uow = CreateUnitOfWork(context);
            // Doctor 202 is from Institution 2, attempting to update appeal 701 (Institution 1)
            var userMock = CreateUserMock(isSAdmin: false, institutionId: 2, role: enUserRole.Doctor, userId: 202);

            var handler = new UpdateGradeAppealRequestCommandHandler(uow, userMock.Object);
            var command = new UpdateGradeAppealRequestCommand(
                701,
                202,
                new UpdateGradeAppealRequestDto { Status = enGradeAppealStatus.Approved, DoctorResponse = "Cross-tenant attack" }
            );

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task UpdateAppeal_AssignedDoctor_ValidTransition_Succeeds()
        {
            await using var context = TestDbContextFactory.Create();
            await SeedTwoInstitutionsDataAsync(context);
            var uow = CreateUnitOfWork(context);
            // Doctor 201 updates Pending -> Approved
            var userMock = CreateUserMock(isSAdmin: false, institutionId: 1, role: enUserRole.Doctor, userId: 201);

            var handler = new UpdateGradeAppealRequestCommandHandler(uow, userMock.Object);
            var command = new UpdateGradeAppealRequestCommand(
                701,
                201,
                new UpdateGradeAppealRequestDto { Status = enGradeAppealStatus.Approved, DoctorResponse = "Approved after review" }
            );

            var result = await handler.Handle(command, CancellationToken.None);
            Assert.True(result);

            var updated = await context.GradeAppealRequests.FindAsync(701);
            Assert.NotNull(updated);
            Assert.Equal(enGradeAppealStatus.Approved, updated.Status);
            Assert.Equal("Approved after review", updated.DoctorResponse);
            Assert.NotNull(updated.ReviewedAt);
        }

        #endregion
    }
}
