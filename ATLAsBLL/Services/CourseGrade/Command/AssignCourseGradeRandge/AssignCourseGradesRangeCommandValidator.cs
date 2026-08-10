using FluentValidation;

namespace ATLAsBLL.Features.CourseGrades.Commands.CreateCourseGradesRange
{
    public class AssignCourseGradesRangeCommandValidator
        : AbstractValidator<AssignCourseGradesRangeCommand>
    {
        public AssignCourseGradesRangeCommandValidator()
        {
            RuleFor(x => x.GradeDTO)
                .NotNull()
                .NotEmpty()
                .WithMessage("Grades list cannot be empty.");

            RuleFor(x => x.GradeDTO)
                .Must(grades => grades
                    .Select(g => (g.EnrollmentId, g.ModuleId))
                    .Distinct()
                    .Count() == grades.Count)
                .WithMessage("A grade batch cannot contain the same enrollment and module more than once.");

            RuleForEach(x => x.GradeDTO).ChildRules(grade =>
            {
                grade.RuleFor(g => g.EnrollmentId)
                    .GreaterThan(0)
                    .WithMessage("Invalid Enrollment ID.");

                grade.RuleFor(g => g.ModuleId)
                    .GreaterThan(0)
                    .WithMessage("Invalid Module ID.");

                grade.RuleFor(g => g.UserId)
                    .GreaterThan(0)
                    .WithMessage("Invalid User ID.");

                grade.RuleFor(g => g.StudentDegree)
                    .InclusiveBetween(0, 100)
                    .WithMessage("Student degree must be between 0 and 100.");
            });
        }
    }
}
