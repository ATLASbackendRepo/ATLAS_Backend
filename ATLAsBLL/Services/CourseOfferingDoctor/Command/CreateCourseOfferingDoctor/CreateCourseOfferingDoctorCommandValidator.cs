using FluentValidation;

namespace ATLAsBLL.Features.CourseOfferingDoctors.Commands.CreateCourseOfferingDoctor
{
    /// <summary>
    /// Validator for CreateCourseOfferingDoctorCommand.
    /// </summary>
    public class CreateCourseOfferingDoctorCommandValidator : AbstractValidator<CreateCourseOfferingDoctorCommand>
    {
        public CreateCourseOfferingDoctorCommandValidator()
        {
            RuleFor(x => x.CourseOfferingId)
                .GreaterThan(0)
                .WithMessage("Course offering ID must be greater than zero.");

            RuleFor(x => x.DoctorId)
                .GreaterThan(0)
                .WithMessage("Doctor ID must be greater than zero.");
        }
    }
}

