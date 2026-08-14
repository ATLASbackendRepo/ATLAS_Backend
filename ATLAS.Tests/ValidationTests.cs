using ATLAsBLL.Features.CourseGrades.Commands.CreateCourseGradesRange;
using ATLAsBLL.Features.Enrollments.Commands.CreateEnrollment;
using ATLAsBLL.Features.Enrollments.Commands.CreateEnrollmentsRange;
using ATLAsBLL.Services.Request.Command.UpdateGradeAppealRequest;
using ATLASDomain.DTO.CourseGrade;
using ATLASDomain.DTO.Request;
using ATLASDomain.Entities;
using Xunit;

namespace ATLAS.Tests;

public class ValidationTests
{
    [Fact]
    public void EnrollmentValidation_RejectsInvalidIdentifiers()
    {
        var result = new CreateEnrollmentCommandValidator().Validate(new CreateEnrollmentCommand(0, -1));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void EnrollmentRangeValidation_RejectsDuplicatePairs()
    {
        var result = new CreateEnrollmentsRangeCommandValidator().Validate(
            new CreateEnrollmentsRangeCommand([new(1, 5), new(1, 5)]));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void GradeRangeValidation_RejectsDuplicatePairs()
    {
        var result = new AssignCourseGradesRangeCommandValidator().Validate(
            new AssignCourseGradesRangeCommand([
                new InsertGradeDTO { EnrollmentId = 1, ModuleId = 2, UserId = 3, StudentDegree = 10 },
                new InsertGradeDTO { EnrollmentId = 1, ModuleId = 2, UserId = 3, StudentDegree = 20 }]));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void GradeAppealValidation_RejectsUndefinedStatus()
    {
        var result = new UpdateGradeAppealRequestCommandValidator().Validate(
            new UpdateGradeAppealRequestCommand(1, 2, new UpdateGradeAppealRequestDto
            {
                Status = (enGradeAppealStatus)99
            }));
        Assert.False(result.IsValid);
    }
}
