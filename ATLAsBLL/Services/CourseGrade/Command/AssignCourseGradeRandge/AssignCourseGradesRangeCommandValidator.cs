using FluentValidation;

namespace ATLAsBLL.Features.CourseGrades.Commands.CreateCourseGradesRange;

/// <summary>
/// Assign Course Grades Range Command Validator.
/// </summary>
public class AssignCourseGradesRangeCommandValidator
    : AbstractValidator<AssignCourseGradesRangeCommand>
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="AssignCourseGradesRangeCommandValidator"/> class.
    /// </summary>
    public AssignCourseGradesRangeCommandValidator()
    {
        RuleFor(command => command.FilePath)
            .NotEmpty()
            .WithMessage("Excel file path is required.");

        RuleFor(command => command.CourseOfferingID)
            .GreaterThan(0)
            .WithMessage("Invalid course offering ID.");

        RuleFor(command => command.ModelID)
            .GreaterThan(0)
            .WithMessage("Invalid module ID.");

        RuleFor(command => command.UserID)
            .GreaterThan(0)
            .WithMessage("The acting user ID is required.");
    }
}
