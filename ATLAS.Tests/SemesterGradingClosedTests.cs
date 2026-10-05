using ATLAsBLL.Features.Semesters.Commands.UpdateSemester;
using ATLAsDCL.Repositories;
using ATLAsDCL.UnitOfWork;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
using Moq;
using Xunit;

namespace ATLAS.Tests;

/// <summary>
/// MED-07: a semester carries the flag that closes its grading, and that flag is
/// what both grade-assignment handlers consult before they write.
/// </summary>
public class SemesterGradingClosedTests
{
    /// <summary>
    /// An open semester reports open grading for its offering.
    /// </summary>
    [Fact]
    public async Task IsGradingClosedForOffering_OpenSemester_ReportsOpenGrading()
    {
        await using var context = TestDbContextFactory.Create();
        context.Semesters.Add(new Semester
        {
            SemesterID = 1,
            SemesterName = "Spring",
            AcademicYear = "2024"
        });
        context.CourseOfferings.Add(new CourseOffering { CourseOfferingID = 10, CourseID = 1, SemesterID = 1 });
        await context.SaveChangesAsync();

        var repository = new SemesterRepository(context);

        Assert.False(await repository.IsGradingClosedForOfferingAsync(10));
    }

    /// <summary>
    /// A closed semester reports closed grading for its offering.
    /// </summary>
    [Fact]
    public async Task IsGradingClosedForOffering_ClosedSemester_ReportsClosedGrading()
    {
        await using var context = TestDbContextFactory.Create();
        context.Semesters.Add(new Semester
        {
            SemesterID = 1,
            SemesterName = "Spring",
            AcademicYear = "2024",
            IsGradingClosed = true
        });
        context.CourseOfferings.Add(new CourseOffering { CourseOfferingID = 10, CourseID = 1, SemesterID = 1 });
        context.CourseOfferings.Add(new CourseOffering { CourseOfferingID = 11, CourseID = 1, SemesterID = 2 });
        await context.SaveChangesAsync();

        var repository = new SemesterRepository(context);

        Assert.True(await repository.IsGradingClosedForOfferingAsync(10));
    }

    /// <summary>
    /// An offering whose semester row is missing cannot be closed, so grading
    /// stays open instead of blocking every grade write on such data.
    /// </summary>
    [Fact]
    public async Task IsGradingClosedForOffering_MissingSemester_ReportsOpenGrading()
    {
        await using var context = TestDbContextFactory.Create();
        context.CourseOfferings.Add(new CourseOffering { CourseOfferingID = 10, CourseID = 1, SemesterID = 404 });
        await context.SaveChangesAsync();

        var repository = new SemesterRepository(context);

        Assert.False(await repository.IsGradingClosedForOfferingAsync(10));
    }

    /// <summary>
    /// Updating a semester persists the grading flag, which is how grading gets
    /// closed for a whole semester.
    /// </summary>
    [Fact]
    public async Task UpdateSemesterCommand_PersistsTheGradingClosedFlag()
    {
        var semester = new Semester
        {
            SemesterID = 1,
            SemesterName = "Spring",
            AcademicYear = "2024"
        };

        var semesters = new Mock<ISemesterRepository>();
        semesters
            .Setup(repository => repository.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(semester);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork
            .SetupGet(work => work.Semesters)
            .Returns(semesters.Object);
        unitOfWork
            .Setup(work => work.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var updated = await new UpdateSemesterCommandHandler(unitOfWork.Object)
            .Handle(
                new UpdateSemesterCommand(1, "Spring", "2024", IsGradingClosed: true),
                CancellationToken.None);

        Assert.True(updated);
        Assert.True(semester.IsGradingClosed);
    }
}
