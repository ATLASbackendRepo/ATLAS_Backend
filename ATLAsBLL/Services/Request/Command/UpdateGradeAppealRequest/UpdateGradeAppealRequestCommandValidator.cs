using FluentValidation;

namespace ATLAsBLL.Services.Request.Command.UpdateGradeAppealRequest
{

    /// <summary>
    /// Update Grade Appeal Request Command Validator.
    /// </summary>
    public class UpdateGradeAppealRequestCommandValidator
        : AbstractValidator<UpdateGradeAppealRequestCommand>
    {
        public UpdateGradeAppealRequestCommandValidator()
        {
            RuleFor(x => x.RequestId)
                .GreaterThan(0);

            RuleFor(x => x.DoctorUserId)
                .GreaterThan(0);

            RuleFor(x => x.Request)
                .NotNull();

            RuleFor(x => x.Request.Status)
                .IsInEnum();

            RuleFor(x => x.Request.DoctorResponse)
                .MaximumLength(2000)
                .When(x => !string.IsNullOrWhiteSpace(x.Request.DoctorResponse));
        }
    }
}
