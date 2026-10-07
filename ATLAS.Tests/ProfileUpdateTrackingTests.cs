using ATLAS.Domain.Entities;
using ATLAS.Persistence.Repositories;
using ATLASDomain.DTO.Profile;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
using ATLASDomain.InterFaces.Repositories;
using ATLAsBLL.Services.profile.Staff.command;
using ATLAsBLL.Services.profile.Student.command;
using ATLAsDCL.Data;
using ATLAsDCL.Repositories;
using ATLAsDCL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ATLAS.Tests;

/// <summary>
/// Covers HIGH-07. A profile update must change only the properties it edits.
/// The profile repositories used to return a detached projection which the
/// handlers pushed back with Update, so EF marked every column as modified:
/// the profile's owner id and CreatedAt were rewritten, and the user's password,
/// refresh-token hash and activation flag were overwritten with stale values.
/// </summary>
public class ProfileUpdateTrackingTests
{
    /// <summary>
    /// A staff profile update keeps the user's credentials and the profile's
    /// owner and creation timestamp.
    /// </summary>
    [Fact]
    public async Task StaffProfileUpdate_KeepsCredentialsOwnerAndCreatedAt()
    {
        await using var context = TestDbContextFactory.Create();
        var createdAt = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);

        var user = new User
        {
            UserId = 5,
            AcademicId = "doc01",
            FullName = "Before",
            Password = "stored-password-hash",
            RefreshTokenHash = "stored-refresh-token-hash",
            Role = enUserRole.Doctor,
            IsActive = true,
            InstitutionID = 2,
            DepartmentId = 3
        };

        context.Users.Add(user);
        context.StaffProfiles.Add(new StaffProfile
        {
            StaffProfileID = 9,
            UserID = user.UserId,
            User = user,
            AcademicId = user.AcademicId,
            Role = "Doctor",
            DepartmentID = 3,
            TeachingYears = "1",
            CreatedAt = createdAt
        });

        await context.SaveChangesAsync();

        var handler = new updateStaffProfileInfoHandler(CreateUnitOfWork(context));

        var updated = await handler.Handle(
            new updateStaffProfileInfoCommand(
                user.UserId,
                new StaffProfileUpdateDTO
                {
                    TeachingYears = "7"
                }),
            CancellationToken.None);

        Assert.True(updated);

        var storedUser = await context.Users.SingleAsync(candidate => candidate.UserId == user.UserId);

        Assert.Equal("Before", storedUser.FullName);
        Assert.Equal("stored-password-hash", storedUser.Password);
        Assert.Equal("stored-refresh-token-hash", storedUser.RefreshTokenHash);
        Assert.True(storedUser.IsActive);

        var storedProfile = await context.StaffProfiles.SingleAsync(profile => profile.StaffProfileID == 9);

        Assert.Equal(user.UserId, storedProfile.UserID);
        Assert.Equal(createdAt, storedProfile.CreatedAt);
        Assert.Equal(3, storedProfile.DepartmentID);
        Assert.Equal("7", storedProfile.TeachingYears);
    }

    /// <summary>
    /// A student profile update keeps the user's credentials and the student and
    /// profile owner identifiers.
    /// </summary>
    [Fact]
    public async Task StudentProfileUpdate_KeepsCredentialsStudentIdAndCreatedAt()
    {
        await using var context = TestDbContextFactory.Create();
        var createdAt = new DateTime(2024, 2, 3, 4, 5, 6, DateTimeKind.Utc);

        var user = new User
        {
            UserId = 7,
            AcademicId = "stu01",
            FullName = "Before",
            Password = "stored-password-hash",
            RefreshTokenHash = "stored-refresh-token-hash",
            Role = enUserRole.Student,
            IsActive = true,
            InstitutionID = 1,
            DepartmentId = 2
        };

        context.Users.Add(user);

        var student = new Student
        {
            StudentId = 11,
            UserId = user.UserId,
            User = user,
            StudentCode = "S00000011",
            AcademicId = user.AcademicId,
            AcademicLevel = 1,
            IsActive = true,
            InstitutionID = 1,
            DepartmentId = 2
        };

        context.Students.Add(student);

        context.StudentProfiles.Add(new StudentProfile
        {
            StudentProfileID = 21,
            StudentID = student.StudentId,
            Student = student,
            Faculty = "Old Faculty",
            SectionNumber = 1,
            CreatedAt = createdAt
        });

        await context.SaveChangesAsync();

        var handler = new updateStudentProfileInfoHandler(CreateUnitOfWork(context));

        var updated = await handler.Handle(
            new updateStudentProfileInfoCommand(
                user.UserId,
                new StudentUpdateProfileInfoDTO
                {
                    LinkedInUrl = "https://example.test/stu01"
                }),
            CancellationToken.None);

        Assert.True(updated);

        var storedUser = await context.Users.SingleAsync(candidate => candidate.UserId == user.UserId);

        Assert.Equal("Before", storedUser.FullName);
        Assert.Equal("stored-password-hash", storedUser.Password);
        Assert.Equal("stored-refresh-token-hash", storedUser.RefreshTokenHash);
        Assert.True(storedUser.IsActive);

        var storedStudent = await context.Students.SingleAsync(candidate => candidate.StudentId == student.StudentId);

        Assert.Equal(user.UserId, storedStudent.UserId);
        Assert.Equal("S00000011", storedStudent.StudentCode);

        var storedProfile = await context.StudentProfiles.SingleAsync(profile => profile.StudentProfileID == 21);

        Assert.Equal(student.StudentId, storedProfile.StudentID);
        Assert.Equal(createdAt, storedProfile.CreatedAt);
        Assert.Equal("Old Faculty", storedProfile.Faculty);
        Assert.Equal(1, storedProfile.SectionNumber);
        Assert.Equal("https://example.test/stu01", storedProfile.LinkedInUrl);
    }

    /// <summary>
    /// Builds a unit of work over the in-memory database, mirroring the
    /// registration used by the application.
    /// </summary>
    /// <param name="context">Context to wrap.</param>
    /// <returns>The unit of work.</returns>
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
        new institutionRepository(context));
}
