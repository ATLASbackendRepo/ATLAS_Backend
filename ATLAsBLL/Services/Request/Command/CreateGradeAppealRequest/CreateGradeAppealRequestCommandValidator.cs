using FluentValidation;

namespace ATLAsBLL.Services.Request.Command.CreateGradeAppealRequest
{

    public class CreateGradeAppealRequestCommandValidator
        : AbstractValidator<CreateGradeAppealRequestCommand>
    {
        public CreateGradeAppealRequestCommandValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0);

            RuleFor(x => x.Request)
                .NotNull();

            RuleFor(x => x.Request.CourseOfferingID)
                .GreaterThan(0);

            RuleFor(x => x.Request.DoctorUserID)
                .GreaterThan(0);

            RuleFor(x => x.Request.AssessmentModuleID)
                .GreaterThan(0)
                .When(x => x.Request.AssessmentModuleID.HasValue);

            RuleFor(x => x.Request.CurrentGrade)
                .InclusiveBetween(0, 40)
                .When(x => x.Request.CurrentGrade.HasValue);

            RuleFor(x => x.Request.ProblemDescription)
                .NotEmpty()
                .MaximumLength(2000);
        }
    }
}
