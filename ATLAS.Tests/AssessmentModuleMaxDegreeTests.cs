using ATLAsBLL.Features.AssessmentModules.Commands.CreateAssessmentModule;
using ATLAsBLL.Features.AssessmentModules.Commands.UpdateAssessmentModule;
using ATLAsBLL.Services.AssessmentModule.Command.CreateAssessmentModule;
using ATLAsDCL.UnitOfWork;
using ATLASDomain.DTO.AssessmentModule;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
using Moq;
using Xunit;

namespace ATLAS.Tests;

/// <summary>
/// MED-07: the application and the database must agree on the module max degree
/// (<see cref="AssessmentModule.MinimumMaxDegree"/> ..
/// <see cref="AssessmentModule.MaximumMaxDegree"/>), the modules of one course
/// offering may not add up to more than
/// <see cref="AssessmentModule.MaximumOfferingTotalMaxDegree"/>, and a max degree
/// may not be lowered below a grade that is already recorded.
/// </summary>
public class AssessmentModuleMaxDegreeTests
{
    private const int UserId = 7;
    private const int CourseOfferingId = 10;
    private const int ModuleId = 55;

    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IAssessmentModuleRepository> _modules = new();
    private readonly Mock<ICourseOfferingDoctorRepository> _doctors = new();
    private readonly Mock<ICourseOfferingTARepository> _teachingAssistants = new();

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="AssessmentModuleMaxDegreeTests"/> class.
    /// </summary>
    public AssessmentModuleMaxDegreeTests()
    {
        _unitOfWork
            .SetupGet(work => work.AssessmentModules)
            .Returns(_modules.Object);

        _unitOfWork
            .SetupGet(work => work.CourseOfferingDoctors)
            .Returns(_doctors.Object);

        _unitOfWork
            .SetupGet(work => work.CourseOfferingTAs)
            .Returns(_teachingAssistants.Object);

        _unitOfWork
            .Setup(work => work.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // The caller is the assigned doctor, so the TA lookup is not consulted.
        _doctors
            .Setup(repository => repository.GetDoctorsByOfferingAsync(CourseOfferingId))
            .ReturnsAsync(new List<int> { UserId });

        _teachingAssistants
            .Setup(repository => repository.GetTAsByOfferingAsync(CourseOfferingId))
            .ReturnsAsync(new List<CourseOfferingTA>());

        _modules
            .Setup(repository => repository.AddAsync(It.IsAny<AssessmentModule>()))
            .Returns(Task.CompletedTask);
    }

    /// <summary>
    /// The reported limits are the ones the database check constraint enforces.
    /// </summary>
    [Fact]
    public void Validator_ReportsTheRangeTheDatabaseEnforces()
    {
        var validator = new CreateAssessmentModuleCommandValidator();

        var result = validator.Validate(new CreateAssessmentModuleCommand(
            CreateDto(maxDegree: 50),
            UserId));

        var message = Assert.Single(result.Errors).ErrorMessage;

        Assert.False(result.IsValid);
        Assert.Contains(
            $"{AssessmentModule.MinimumMaxDegree} and {AssessmentModule.MaximumMaxDegree}",
            message);
    }

    /// <summary>
    /// The upper boundary of the range is accepted.
    /// </summary>
    [Fact]
    public void Validator_AcceptsTheMaximumMaxDegree()
    {
        var validator = new CreateAssessmentModuleCommandValidator();

        var result = validator.Validate(new CreateAssessmentModuleCommand(
            CreateDto(maxDegree: AssessmentModule.MaximumMaxDegree),
            UserId));

        Assert.True(result.IsValid);
    }

    /// <summary>
    /// The handler repeats the range check, so a request that skipped validation
    /// still cannot store a value the database would reject.
    /// </summary>
    [Fact]
    public async Task Handle_RejectsAMaxDegreeAboveTheDatabaseLimit()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            CreateHandler().Handle(
                new CreateAssessmentModuleCommand(CreateDto(maxDegree: 50), UserId),
                CancellationToken.None));

        Assert.Contains(
            $"{AssessmentModule.MinimumMaxDegree} and {AssessmentModule.MaximumMaxDegree}",
            exception.Message);

        AssertNothingWasWritten();
    }

    /// <summary>
    /// A module that would push the offering total over the cap is rejected and
    /// nothing is written.
    /// </summary>
    [Fact]
    public async Task Handle_RejectsWhenTheOfferingTotalWouldExceedTheCap()
    {
        SetupOfferingTotal(80);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            CreateHandler().Handle(
                new CreateAssessmentModuleCommand(CreateDto(maxDegree: 30), UserId),
                CancellationToken.None));

        Assert.Contains(
            $"{AssessmentModule.MaximumOfferingTotalMaxDegree}",
            exception.Message);

        AssertNothingWasWritten();
    }

    /// <summary>
    /// A total that lands exactly on the cap is accepted.
    /// </summary>
    [Fact]
    public async Task Handle_AcceptsATotalThatLandsExactlyOnTheCap()
    {
        SetupOfferingTotal(70);
        AssessmentModule? added = null;

        _modules
            .Setup(repository => repository.AddAsync(It.IsAny<AssessmentModule>()))
            .Callback<AssessmentModule>(module => added = module)
            .Returns(Task.CompletedTask);

        await CreateHandler().Handle(
            new CreateAssessmentModuleCommand(CreateDto(maxDegree: 30), UserId),
            CancellationToken.None);

        Assert.NotNull(added);
        Assert.Equal(30m, added.MaxDegree);
        _unitOfWork.Verify(
            work => work.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
    /// <summary>
    /// Lowering the max degree below a grade that already exists would leave the
    /// recorded grade above the module's maximum, so it is rejected.
    /// </summary>
    [Fact]
    public async Task Update_RejectsLoweringBelowARecordedGrade()
    {
        SetupExistingModule(maxDegree: 40);
        SetupRecordedMaxDegree(35);
        SetupOfferingTotal(40);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            UpdateHandler().Handle(
                new UpdateAssessmentModuleCommand(UpdateDto(maxDegree: 30), UserId),
                CancellationToken.None));

        Assert.Contains("35", exception.Message);

        _unitOfWork.Verify(
            work => work.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// Lowering the max degree to exactly the highest recorded grade stays
    /// possible.
    /// </summary>
    [Fact]
    public async Task Update_AllowsLoweringToTheHighestRecordedGrade()
    {
        SetupExistingModule(maxDegree: 40);
        SetupRecordedMaxDegree(35);
        SetupOfferingTotal(40);

        var updated = await UpdateHandler().Handle(
            new UpdateAssessmentModuleCommand(UpdateDto(maxDegree: 35), UserId),
            CancellationToken.None);

        Assert.True(updated);
        _unitOfWork.Verify(
            work => work.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// An update that would push the offering total over the cap is rejected, and
    /// the module's own previous value is not counted twice.
    /// </summary>
    [Fact]
    public async Task Update_RejectsWhenTheOfferingTotalWouldExceedTheCap()
    {
        SetupExistingModule(maxDegree: 20);
        SetupRecordedMaxDegree(0);

        // The offering already holds 90 degrees, of which 20 belong to this
        // module: the other modules leave room for 10 more degrees only.
        SetupOfferingTotal(90);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            UpdateHandler().Handle(
                new UpdateAssessmentModuleCommand(UpdateDto(maxDegree: 40), UserId),
                CancellationToken.None));

        Assert.Contains(
            $"{AssessmentModule.MaximumOfferingTotalMaxDegree}",
            exception.Message);

        _unitOfWork.Verify(
            work => work.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// An update that keeps the offering total within the cap is saved.
    /// </summary>
    [Fact]
    public async Task Update_AllowsATotalThatStaysWithinTheCap()
    {
        SetupExistingModule(maxDegree: 20);
        SetupRecordedMaxDegree(0);
        SetupOfferingTotal(90);

        var updated = await UpdateHandler().Handle(
            new UpdateAssessmentModuleCommand(UpdateDto(maxDegree: 30), UserId),
            CancellationToken.None);

        Assert.True(updated);
        _unitOfWork.Verify(
            work => work.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Creates the handler under test.
    /// </summary>
    private CreateAssessmentModuleCommandHandler CreateHandler()
        => new(_unitOfWork.Object);

    /// <summary>
    /// Creates the handler under test.
    /// </summary>
    private UpdateAssessmentModuleCommandHandler UpdateHandler()
        => new(_unitOfWork.Object);

    /// <summary>
    /// Builds a create request.
    /// </summary>
    private static CreateModeDTO CreateDto(decimal maxDegree)
        => new()
        {
            CourseOfferingId = CourseOfferingId,
            ModuleName = "Midterm",
            MaxDegree = maxDegree
        };

    /// <summary>
    /// Builds an update request.
    /// </summary>
    private static UpdateModeDTO UpdateDto(decimal maxDegree)
        => new()
        {
            ModuleId = ModuleId,
            ModuleName = "Midterm",
            MaxDegree = maxDegree
        };

    /// <summary>
    /// Registers the module the update handler resolves by id.
    /// </summary>
    private void SetupExistingModule(decimal maxDegree)
    {
        _modules
            .Setup(repository => repository.GetByIdAsync(ModuleId))
            .ReturnsAsync(new AssessmentModule
            {
                ModuleID = ModuleId,
                CourseOfferingID = CourseOfferingId,
                ModuleName = "Midterm",
                MaxDegree = maxDegree
            });
    }

    /// <summary>
    /// Registers the sum of the module max degrees of the offering.
    /// </summary>
    private void SetupOfferingTotal(decimal total)
    {
        _modules
            .Setup(repository => repository.GetTotalMaxDegreeByCourseOfferingAsync(CourseOfferingId))
            .ReturnsAsync(total);
    }

    /// <summary>
    /// Registers the highest degree already recorded for the module.
    /// </summary>
    private void SetupRecordedMaxDegree(decimal maxDegree)
    {
        _modules
            .Setup(repository => repository.GetMaxDegreeByModuleAsync(ModuleId))
            .ReturnsAsync(maxDegree);
    }

    /// <summary>
    /// Asserts that the request wrote nothing.
    /// </summary>
    private void AssertNothingWasWritten()
    {
        _modules.Verify(
            repository => repository.AddAsync(It.IsAny<AssessmentModule>()),
            Times.Never);

        _unitOfWork.Verify(
            work => work.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}