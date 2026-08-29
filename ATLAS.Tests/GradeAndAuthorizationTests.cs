using ATLAS;
using ATLAS.Persistence.Repositories;
using ATLAsBLL.Features.CourseGrades.Commands.CreateCourseGradesRange;
using ATLAsBLL.Features.CourseGrades.Commands.UpdateCourseGrade;
using ATLAsBLL.Features.Enrollments.Commands.CreateEnrollmentsRange;
using ATLAsBLL.Services.Enrollment.Command.CreateEnrollmentRange;
using ATLAsDCL.Data;
using ATLAsDCL.Repositories;
using ATLAsDCL.UnitOfWork;
using ATLASDomain.DTO.CourseGrade;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
using ATLASDomain.InterFaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Xunit;

namespace ATLAS.Tests;

public class GradeAndAuthorizationTests
{
    [Fact]
    public async Task AssignGrade_InsertsThenUpdatesTheSameGrade()
    {
        await using var context = TestDbContextFactory.Create();
        SeedGradeData(context);
        var unitOfWork = CreateUnitOfWork(context);
        var handler = new AssignCourseGradeCommandHandler(unitOfWork);

        await handler.Handle(new AssignCourseGradeCommand(100, 200, 60, 9), CancellationToken.None);
        await handler.Handle(new AssignCourseGradeCommand(100, 200, 75, 10), CancellationToken.None);

        var grade = Assert.Single(context.CourseGrades);
        Assert.Equal(75, grade.StudentDegree);
        Assert.Equal(9, grade.CreatedBy);
        Assert.Equal(10, grade.UpdatedBy);
    }

    [Fact]
    public async Task BatchGradeAssignment_RejectsDuplicateEnrollmentModulePairs()
    {
        var handler = new AssignCourseGradesRangeCommandHandler(null!, null!);
        var request = new AssignCourseGradesRangeCommand(new List<InsertGradeDTO>
        {
            new() { EnrollmentId = 100, ModuleId = 200, StudentDegree = 50, UserId = 9 },
            new() { EnrollmentId = 100, ModuleId = 200, StudentDegree = 60, UserId = 9 }
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task BatchEnrollment_RejectsDuplicateStudentOfferingPairs()
    {
        var handler = new CreateEnrollmentsRangeCommandHandler(null!);
        var request = new CreateEnrollmentsRangeCommand(new List<EnrollmentItemDto>
        {
            new(1, 10), new(1, 10)
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task DoctorCanManageOnlyAssignedOfferingAndMatchingModule()
    {
        await using var context = TestDbContextFactory.Create();
        context.CourseOfferings.Add(new CourseOffering { CourseOfferingID = 10, CourseID = 1, SemesterID = 1 });
        context.CourseOfferingDoctors.Add(new CourseOfferingDoctor { CourseOfferingDoctorID = 1, CourseOfferingID = 10, DoctorID = 7 });
        context.Enrollments.Add(new Enrollment { EnrollmentID = 100, StudentID = 1, CourseOfferingID = 10 });
        context.AssessmentModules.AddRange(
            new AssessmentModule { ModuleID = 200, CourseOfferingID = 10, ModuleName = "Final", MaxDegree = 100 },
            new AssessmentModule { ModuleID = 201, CourseOfferingID = 11, ModuleName = "Other", MaxDegree = 100 });
        await context.SaveChangesAsync();
        var service = new CourseOfferingAuthorizationService(context);
        var doctor = Principal(7, "Doctor");

        Assert.True(await service.CanManageGradeAsync(doctor, 100, 200));
        Assert.False(await service.CanManageGradeAsync(doctor, 100, 201));
        Assert.False(await service.CanAccessOfferingAsync(Principal(8, "Doctor"), 10));
    }

    private static ClaimsPrincipal Principal(int userId, string role) => new(
        new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId.ToString()), new Claim(ClaimTypes.Role, role)],
            "test"));

    private static void SeedGradeData(AtLASDbContext context)
    {
        context.CourseOfferings.Add(new CourseOffering { CourseOfferingID = 10, CourseID = 1, SemesterID = 1 });
        context.Enrollments.Add(new Enrollment { EnrollmentID = 100, StudentID = 1, CourseOfferingID = 10 });
        context.AssessmentModules.Add(new AssessmentModule { ModuleID = 200, CourseOfferingID = 10, ModuleName = "Final", MaxDegree = 100 });
        context.SaveChanges();
    }

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
        new StaffProfileRepository(context) ,
        new CourseGroupMessageReadRepository(context),
       new   GradeAppealRequestRepository(context)
    );
}
