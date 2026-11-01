using ATLASDomain.DTO.Course;
using ATLASDomain.DTO.CourseOffring;
using ATLASDomain.Entities;
using ATLAsBLL.Exceptions;
using ATLAsBLL.Features.Courses.Commands.CreateCourse;
using ATLAsBLL.Features.Courses.Commands.DeleteCourse;
using ATLAsBLL.Features.Courses.Commands.UpdateCourse;
using ATLAsBLL.Features.CourseOfferings.Commands.CreateCourseOffering;
using ATLAsBLL.Features.CourseOfferings.Commands.DeleteCourseOffering;
using ATLAsBLL.Features.CourseOfferings.Commands.UpdateCourseOffering;
using ATLAsBLL.Services.Course.Queries;
using ATLAsBLL.Services.CourseOffering.Queries.GetCourseDetails;
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
    public class CourseAndOfferingTenantScopingTests
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

        private static Mock<ICurrentUserService> CreateUserMock(bool isSAdmin, int? institutionId, enUserRole role = enUserRole.Admin, int userId = 1)
        {
            var mock = new Mock<ICurrentUserService>();
            mock.Setup(u => u.IsSAdmin).Returns(isSAdmin);
            mock.Setup(u => u.InstitutionId).Returns(institutionId);
            mock.Setup(u => u.Role).Returns(role);
            mock.Setup(u => u.UserId).Returns(userId);
            return mock;
        }

        #region Course Scoping Tests (BE-04)

        [Fact]
        public async Task CreateCourse_Admin_WithMissingInstitutionClaim_ThrowsForbiddenAccessException()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);
            var userMock = CreateUserMock(isSAdmin: false, institutionId: null);

            var handler = new CreateCourseCommandHandler(uow, userMock.Object);
            var cmd = new CreateCourseCommand(new CreateCourseDTO
            {
                CourseName = "CS101",
                CreditHours = 3,
                IsPractical = true
            });

            await Assert.ThrowsAsync<ForbiddenAccessException>(() => handler.Handle(cmd, CancellationToken.None));
        }

        [Fact]
        public async Task CreateCourse_Admin_WithValidInstitutionClaim_SetsInstitutionIdCorrectly()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);
            var userMock = CreateUserMock(isSAdmin: false, institutionId: 10);

            var handler = new CreateCourseCommandHandler(uow, userMock.Object);
            var cmd = new CreateCourseCommand(new CreateCourseDTO
            {
                CourseName = "CS101",
                CreditHours = 3,
                IsPractical = true
            });

            var courseId = await handler.Handle(cmd, CancellationToken.None);
            Assert.True(courseId > 0);

            var created = await uow.Courses.GetByIdAsync(courseId);
            Assert.NotNull(created);
            Assert.Equal("CS101", created.C_Name);
            Assert.Equal(10, created.InstitutionID);
        }

        [Fact]
        public async Task CreateCourse_Admin_DuplicateCourseNameSameInstitution_ThrowsInvalidOperationException()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);
            var userMock = CreateUserMock(isSAdmin: false, institutionId: 10);

            var handler = new CreateCourseCommandHandler(uow, userMock.Object);
            await handler.Handle(new CreateCourseCommand(new CreateCourseDTO
            {
                CourseName = "Math 101",
                CreditHours = 3
            }), CancellationToken.None);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                handler.Handle(new CreateCourseCommand(new CreateCourseDTO
                {
                    CourseName = "math 101", // case-insensitive duplicate
                    CreditHours = 3
                }), CancellationToken.None));

            Assert.Contains("already exists", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CreateCourse_DifferentInstitutions_SameCourseName_Succeeds()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);

            var admin1 = CreateUserMock(isSAdmin: false, institutionId: 1);
            var admin2 = CreateUserMock(isSAdmin: false, institutionId: 2);

            var handler1 = new CreateCourseCommandHandler(uow, admin1.Object);
            var handler2 = new CreateCourseCommandHandler(uow, admin2.Object);

            var c1 = await handler1.Handle(new CreateCourseCommand(new CreateCourseDTO { CourseName = "Physics I", CreditHours = 4 }), CancellationToken.None);
            var c2 = await handler2.Handle(new CreateCourseCommand(new CreateCourseDTO { CourseName = "Physics I", CreditHours = 4 }), CancellationToken.None);

            Assert.NotEqual(c1, c2);
        }

        [Fact]
        public async Task CreateCourse_SAdmin_WithoutExplicitInstitutionId_ThrowsArgumentException()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);
            var sadminMock = CreateUserMock(isSAdmin: true, institutionId: null, role: enUserRole.SAdmin);

            var handler = new CreateCourseCommandHandler(uow, sadminMock.Object);
            var cmd = new CreateCourseCommand(new CreateCourseDTO
            {
                CourseName = "CS201",
                CreditHours = 3,
                InstitutionId = null
            });

            await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(cmd, CancellationToken.None));
        }

        [Fact]
        public async Task CreateCourse_SAdmin_WithExplicitInstitutionId_Succeeds()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);
            var sadminMock = CreateUserMock(isSAdmin: true, institutionId: null, role: enUserRole.SAdmin);

            var handler = new CreateCourseCommandHandler(uow, sadminMock.Object);
            var cmd = new CreateCourseCommand(new CreateCourseDTO
            {
                CourseName = "CS201",
                CreditHours = 3,
                InstitutionId = 55
            });

            var courseId = await handler.Handle(cmd, CancellationToken.None);
            var course = await uow.Courses.GetByIdAsync(courseId);
            Assert.NotNull(course);
            Assert.Equal(55, course.InstitutionID);
        }

        [Fact]
        public async Task UpdateCourse_Admin_SameInstitution_Succeeds()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);
            var adminMock = CreateUserMock(isSAdmin: false, institutionId: 10);

            var course = new Course { C_Name = "Old Name", Credit_Hours = 3, InstitutionID = 10 };
            await uow.Courses.AddAsync(course);
            await uow.SaveChangesAsync();

            var handler = new UpdateCourseCommandHandler(uow, adminMock.Object);
            var result = await handler.Handle(new UpdateCourseCommand(new UpdateCourseDTO
            {
                Id = course.CourseID,
                CourseName = "New Name",
                CreditHours = 4,
                IsPractical = true
            }), CancellationToken.None);

            Assert.True(result);
            var updated = await uow.Courses.GetByIdAsync(course.CourseID);
            Assert.Equal("New Name", updated!.C_Name);
            Assert.Equal(4, updated.Credit_Hours);
        }

        [Fact]
        public async Task UpdateCourse_Admin_CrossTenant_ThrowsForbiddenAccessException()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);
            var adminMock = CreateUserMock(isSAdmin: false, institutionId: 10);

            var foreignCourse = new Course { C_Name = "Foreign Course", Credit_Hours = 3, InstitutionID = 99 };
            await uow.Courses.AddAsync(foreignCourse);
            await uow.SaveChangesAsync();

            var handler = new UpdateCourseCommandHandler(uow, adminMock.Object);
            await Assert.ThrowsAsync<ForbiddenAccessException>(() => handler.Handle(new UpdateCourseCommand(new UpdateCourseDTO
            {
                Id = foreignCourse.CourseID,
                CourseName = "Hacked Name",
                CreditHours = 3
            }), CancellationToken.None));
        }

        [Fact]
        public async Task DeleteCourse_Admin_SameInstitution_Succeeds()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);
            var adminMock = CreateUserMock(isSAdmin: false, institutionId: 10);

            var course = new Course { C_Name = "To Delete", Credit_Hours = 3, InstitutionID = 10 };
            await uow.Courses.AddAsync(course);
            await uow.SaveChangesAsync();

            var handler = new DeleteCourseCommandHandler(uow, adminMock.Object);
            var result = await handler.Handle(new DeleteCourseCommand(course.CourseID), CancellationToken.None);

            Assert.True(result);
            var deleted = await uow.Courses.GetByIdAsync(course.CourseID);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteCourse_Admin_CrossTenant_ThrowsForbiddenAccessException()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);
            var adminMock = CreateUserMock(isSAdmin: false, institutionId: 10);

            var foreignCourse = new Course { C_Name = "Foreign Delete", Credit_Hours = 3, InstitutionID = 99 };
            await uow.Courses.AddAsync(foreignCourse);
            await uow.SaveChangesAsync();

            var handler = new DeleteCourseCommandHandler(uow, adminMock.Object);
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                handler.Handle(new DeleteCourseCommand(foreignCourse.CourseID), CancellationToken.None));
        }

        [Fact]
        public async Task DeleteCourse_Admin_WithOfferings_ThrowsInvalidOperationException()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);
            var adminMock = CreateUserMock(isSAdmin: false, institutionId: 10);

            var course = new Course { C_Name = "Offered Course", Credit_Hours = 3, InstitutionID = 10 };
            var semester = new Semester { SemesterName = "Fall", AcademicYear = "2025-2026", InstitutionID = 10 };
            await uow.Courses.AddAsync(course);
            await uow.Semesters.AddAsync(semester);
            await uow.SaveChangesAsync();

            var offering = new CourseOffering { CourseID = course.CourseID, SemesterID = semester.SemesterID, InstitutionID = 10 };
            await uow.CourseOfferings.AddAsync(offering);
            await uow.SaveChangesAsync();

            var handler = new DeleteCourseCommandHandler(uow, adminMock.Object);
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                handler.Handle(new DeleteCourseCommand(course.CourseID), CancellationToken.None));

            Assert.Contains("associated course offerings", ex.Message);
        }

        [Fact]
        public async Task GetAllCourses_Admin_FiltersOnlyCallerInstitution()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);

            await uow.Courses.AddAsync(new Course { C_Name = "Inst1 Course", Credit_Hours = 3, InstitutionID = 1 });
            await uow.Courses.AddAsync(new Course { C_Name = "Inst2 Course", Credit_Hours = 3, InstitutionID = 2 });
            await uow.Courses.AddAsync(new Course { C_Name = "Null Inst Course", Credit_Hours = 3, InstitutionID = null });
            await uow.SaveChangesAsync();

            var admin1Mock = CreateUserMock(isSAdmin: false, institutionId: 1);
            var handler = new GetAllCourseQueryHandler(uow, admin1Mock.Object);

            var results = await handler.Handle(new GetAllCourseQuery(null), CancellationToken.None);
            Assert.Single(results);
            Assert.Equal("Inst1 Course", results[0].CourseName);
        }

        [Fact]
        public async Task GetAllCourses_SAdmin_ReturnsAllCourses()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);

            await uow.Courses.AddAsync(new Course { C_Name = "Inst1 Course", Credit_Hours = 3, InstitutionID = 1 });
            await uow.Courses.AddAsync(new Course { C_Name = "Inst2 Course", Credit_Hours = 3, InstitutionID = 2 });
            await uow.SaveChangesAsync();

            var sadminMock = CreateUserMock(isSAdmin: true, institutionId: null, role: enUserRole.SAdmin);
            var handler = new GetAllCourseQueryHandler(uow, sadminMock.Object);

            var results = await handler.Handle(new GetAllCourseQuery(null), CancellationToken.None);
            Assert.Equal(2, results.Count);
        }

        #endregion

        #region Course Offering Scoping & Uniqueness Tests (BE-04, BE-17)

        [Fact]
        public async Task CreateCourseOffering_Admin_NoInstitutionClaim_ThrowsForbiddenAccessException()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);
            var userMock = CreateUserMock(isSAdmin: false, institutionId: null);

            var handler = new CreateCourseOfferingCommandHandler(uow, userMock.Object);
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                handler.Handle(new CreateCourseOfferingCommand(1, 1), CancellationToken.None));
        }

        [Fact]
        public async Task CreateCourseOffering_Admin_SameInstitution_Succeeds()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);
            var adminMock = CreateUserMock(isSAdmin: false, institutionId: 5);

            var course = new Course { C_Name = "Algorithms", Credit_Hours = 3, InstitutionID = 5 };
            var semester = new Semester { SemesterName = "Fall", AcademicYear = "2025-2026", InstitutionID = 5 };
            await uow.Courses.AddAsync(course);
            await uow.Semesters.AddAsync(semester);
            await uow.SaveChangesAsync();

            var handler = new CreateCourseOfferingCommandHandler(uow, adminMock.Object);
            var offeringId = await handler.Handle(new CreateCourseOfferingCommand(course.CourseID, semester.SemesterID), CancellationToken.None);

            Assert.True(offeringId > 0);
            var offering = await uow.CourseOfferings.GetByIdAsync(offeringId);
            Assert.NotNull(offering);
            Assert.Equal(5, offering.InstitutionID);
        }

        [Fact]
        public async Task CreateCourseOffering_Admin_CourseFromDifferentInstitution_ThrowsArgumentException()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);
            var adminMock = CreateUserMock(isSAdmin: false, institutionId: 5);

            var foreignCourse = new Course { C_Name = "Foreign Algos", Credit_Hours = 3, InstitutionID = 99 };
            var semester = new Semester { SemesterName = "Fall", AcademicYear = "2025-2026", InstitutionID = 5 };
            await uow.Courses.AddAsync(foreignCourse);
            await uow.Semesters.AddAsync(semester);
            await uow.SaveChangesAsync();

            var handler = new CreateCourseOfferingCommandHandler(uow, adminMock.Object);
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                handler.Handle(new CreateCourseOfferingCommand(foreignCourse.CourseID, semester.SemesterID), CancellationToken.None));

            Assert.Contains("does not belong to the target institution", ex.Message);
        }

        [Fact]
        public async Task CreateCourseOffering_Admin_SemesterFromDifferentInstitution_ThrowsArgumentException()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);
            var adminMock = CreateUserMock(isSAdmin: false, institutionId: 5);

            var course = new Course { C_Name = "Algos", Credit_Hours = 3, InstitutionID = 5 };
            var foreignSemester = new Semester { SemesterName = "Fall", AcademicYear = "2025-2026", InstitutionID = 99 };
            await uow.Courses.AddAsync(course);
            await uow.Semesters.AddAsync(foreignSemester);
            await uow.SaveChangesAsync();

            var handler = new CreateCourseOfferingCommandHandler(uow, adminMock.Object);
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                handler.Handle(new CreateCourseOfferingCommand(course.CourseID, foreignSemester.SemesterID), CancellationToken.None));

            Assert.Contains("does not belong to the target institution", ex.Message);
        }

        [Fact]
        public async Task CreateCourseOffering_DuplicateOfferingInSameSemester_ThrowsInvalidOperationException_BE17()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);
            var adminMock = CreateUserMock(isSAdmin: false, institutionId: 5);

            var course = new Course { C_Name = "Data Structures", Credit_Hours = 3, InstitutionID = 5 };
            var semester = new Semester { SemesterName = "Spring", AcademicYear = "2025-2026", InstitutionID = 5 };
            await uow.Courses.AddAsync(course);
            await uow.Semesters.AddAsync(semester);
            await uow.SaveChangesAsync();

            var handler = new CreateCourseOfferingCommandHandler(uow, adminMock.Object);
            await handler.Handle(new CreateCourseOfferingCommand(course.CourseID, semester.SemesterID), CancellationToken.None);

            // Attempt duplicate creation
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                handler.Handle(new CreateCourseOfferingCommand(course.CourseID, semester.SemesterID), CancellationToken.None));

            Assert.Contains("already exists", ex.Message);
        }

        [Fact]
        public async Task CreateCourseOffering_SAdmin_WithoutExplicitInstitutionId_ThrowsArgumentException()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);
            var sadminMock = CreateUserMock(isSAdmin: true, institutionId: null, role: enUserRole.SAdmin);

            var handler = new CreateCourseOfferingCommandHandler(uow, sadminMock.Object);
            await Assert.ThrowsAsync<ArgumentException>(() =>
                handler.Handle(new CreateCourseOfferingCommand(1, 1, InstitutionId: null), CancellationToken.None));
        }

        [Fact]
        public async Task DeleteCourseOffering_Admin_CrossTenant_ThrowsForbiddenAccessException()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);
            var adminMock = CreateUserMock(isSAdmin: false, institutionId: 5);

            var foreignOffering = new CourseOffering { CourseID = 1, SemesterID = 1, InstitutionID = 99 };
            await uow.CourseOfferings.AddAsync(foreignOffering);
            await uow.SaveChangesAsync();

            var handler = new DeleteCourseOfferingCommandHandler(uow, adminMock.Object);
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                handler.Handle(new DeleteCourseOfferingCommand(foreignOffering.CourseOfferingID), CancellationToken.None));
        }

        [Fact]
        public async Task DeleteCourseOffering_Admin_WithActiveEnrollments_ThrowsInvalidOperationException()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);
            var adminMock = CreateUserMock(isSAdmin: false, institutionId: 5);

            var offering = new CourseOffering { CourseID = 1, SemesterID = 1, InstitutionID = 5 };
            var studentUser = new User
            {
                AcademicId = "STU-001",
                FullName = "Student One",
                Password = "hashedpassword",
                Role = enUserRole.Student,
                InstitutionID = 5
            };
            var student = new Student
            {
                StudentCode = "CODE01",
                AcademicId = "STU-001",
                AcademicLevel = 1,
                DepartmentId = 1,
                InstitutionID = 5,
                User = studentUser
            };
            var enrollment = new Enrollment { CourseOffering = offering, Student = student };
            offering.Enrollments.Add(enrollment);

            await uow.CourseOfferings.AddAsync(offering);
            await uow.Users.AddAsync(studentUser);
            await uow.Students.AddAsync(student);
            await uow.Enrollments.AddAsync(enrollment);
            await uow.SaveChangesAsync();

            var handler = new DeleteCourseOfferingCommandHandler(uow, adminMock.Object);
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                handler.Handle(new DeleteCourseOfferingCommand(offering.CourseOfferingID), CancellationToken.None));

            Assert.Contains("active enrollments", ex.Message);
        }

        [Fact]
        public async Task GetCourseDetails_Admin_CrossTenant_ThrowsForbiddenAccessException()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);

            var adminUser = new User
            {
                AcademicId = "ADM-001",
                FullName = "Admin User",
                Password = "hashedpassword",
                Role = enUserRole.Admin,
                InstitutionID = 1
            };
            await uow.Users.AddAsync(adminUser);

            var foreignOffering = new CourseOffering { CourseID = 1, SemesterID = 1, InstitutionID = 99 };
            await uow.CourseOfferings.AddAsync(foreignOffering);
            await uow.SaveChangesAsync();

            var adminMock = CreateUserMock(isSAdmin: false, institutionId: 1, role: enUserRole.Admin, userId: adminUser.UserId);
            var handler = new GetCourseDetailsQueryHandler(uow, adminMock.Object);

            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                handler.Handle(new GetCourseDetailsQuery(foreignOffering.CourseOfferingID, adminUser.UserId), CancellationToken.None));
        }

        [Fact]
        public async Task GetCourseDetails_Admin_SameInstitution_ReturnsDetails()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);

            var adminUser = new User
            {
                AcademicId = "ADM-001",
                FullName = "Admin User",
                Password = "hashedpassword",
                Role = enUserRole.Admin,
                InstitutionID = 1
            };
            await uow.Users.AddAsync(adminUser);

            var offering = new CourseOffering { CourseID = 1, SemesterID = 1, InstitutionID = 1 };
            await uow.CourseOfferings.AddAsync(offering);
            await uow.SaveChangesAsync();

            var adminMock = CreateUserMock(isSAdmin: false, institutionId: 1, role: enUserRole.Admin, userId: adminUser.UserId);
            var handler = new GetCourseDetailsQueryHandler(uow, adminMock.Object);

            var results = await handler.Handle(new GetCourseDetailsQuery(offering.CourseOfferingID, adminUser.UserId), CancellationToken.None);
            Assert.NotNull(results);
        }

        [Fact]
        public async Task UpdateCourseOffering_Admin_CrossTenant_ThrowsForbiddenAccessException()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);
            var adminMock = CreateUserMock(isSAdmin: false, institutionId: 5);

            var foreignOffering = new CourseOffering { CourseID = 1, SemesterID = 1, InstitutionID = 99 };
            await uow.CourseOfferings.AddAsync(foreignOffering);
            await uow.SaveChangesAsync();

            var handler = new UpdateCourseOfferingCommandHandler(uow, adminMock.Object);
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                handler.Handle(new UpdateCourseOfferingCommand(foreignOffering.CourseOfferingID, 1, 1), CancellationToken.None));
        }

        [Fact]
        public async Task UpdateCourseOffering_Admin_SameInstitution_Succeeds()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);
            var adminMock = CreateUserMock(isSAdmin: false, institutionId: 5);

            var c1 = new Course { C_Name = "C1", Credit_Hours = 3, InstitutionID = 5 };
            var c2 = new Course { C_Name = "C2", Credit_Hours = 3, InstitutionID = 5 };
            var s1 = new Semester { SemesterName = "S1", AcademicYear = "2025", InstitutionID = 5 };
            await uow.Courses.AddAsync(c1);
            await uow.Courses.AddAsync(c2);
            await uow.Semesters.AddAsync(s1);
            await uow.SaveChangesAsync();

            var offering = new CourseOffering { CourseID = c1.CourseID, SemesterID = s1.SemesterID, InstitutionID = 5 };
            await uow.CourseOfferings.AddAsync(offering);
            await uow.SaveChangesAsync();

            var handler = new UpdateCourseOfferingCommandHandler(uow, adminMock.Object);
            var result = await handler.Handle(new UpdateCourseOfferingCommand(offering.CourseOfferingID, c2.CourseID, s1.SemesterID), CancellationToken.None);

            Assert.True(result);
            var updated = await uow.CourseOfferings.GetByIdAsync(offering.CourseOfferingID);
            Assert.Equal(c2.CourseID, updated!.CourseID);
        }

        [Fact]
        public async Task UpdateCourseOffering_SAdmin_CanBackfillUnassignedOffering()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);
            var sadminMock = CreateUserMock(isSAdmin: true, institutionId: null, role: enUserRole.SAdmin);

            var c1 = new Course { C_Name = "C1", Credit_Hours = 3, InstitutionID = 7 };
            var s1 = new Semester { SemesterName = "S1", AcademicYear = "2025", InstitutionID = 7 };
            await uow.Courses.AddAsync(c1);
            await uow.Semesters.AddAsync(s1);
            await uow.SaveChangesAsync();

            // Legacy offering with null InstitutionID
            var legacyOffering = new CourseOffering { CourseID = c1.CourseID, SemesterID = s1.SemesterID, InstitutionID = null };
            await uow.CourseOfferings.AddAsync(legacyOffering);
            await uow.SaveChangesAsync();

            var handler = new UpdateCourseOfferingCommandHandler(uow, sadminMock.Object);
            var result = await handler.Handle(new UpdateCourseOfferingCommand(legacyOffering.CourseOfferingID, c1.CourseID, s1.SemesterID, InstitutionId: 7), CancellationToken.None);

            Assert.True(result);
            var updated = await uow.CourseOfferings.GetByIdAsync(legacyOffering.CourseOfferingID);
            Assert.Equal(7, updated!.InstitutionID);
        }

        [Fact]
        public async Task DeleteCourseOffering_Admin_WithAssignedDoctor_ThrowsInvalidOperationException()
        {
            await using var context = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(context);
            var adminMock = CreateUserMock(isSAdmin: false, institutionId: 5);

            var offering = new CourseOffering { CourseID = 1, SemesterID = 1, InstitutionID = 5 };
            var doctorAssoc = new CourseOfferingDoctor { CourseOffering = offering, DoctorID = 100 };
            offering.CourseOfferingDoctors.Add(doctorAssoc);

            await uow.CourseOfferings.AddAsync(offering);
            await uow.CourseOfferingDoctors.AddAsync(doctorAssoc);
            await uow.SaveChangesAsync();

            var handler = new DeleteCourseOfferingCommandHandler(uow, adminMock.Object);
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                handler.Handle(new DeleteCourseOfferingCommand(offering.CourseOfferingID), CancellationToken.None));

            Assert.Contains("doctors are assigned", ex.Message);
        }

        #endregion
    }
}

