using FluentValidation;
using ATLAsBLL.Services.CourseGroupMessage.Command.SendCourseGroupMessage;

namespace ATLAsBLL.Services.CourseGroupMessage.Command.SendCourseGroupMessage;

public class SendCourseGroupMessageCommandValidator : AbstractValidator<SendCourseGroupMessageCommand>
{
    public SendCourseGroupMessageCommandValidator()
    {
        RuleFor(x => x.CourseOfferingId)
            .NotEmpty()
            .GreaterThan(0)
            .WithMessage("Valid CourseOfferingId is required.");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Message content is required.")
            .MaximumLength(2000)
            .WithMessage("Message content cannot exceed 2000 characters.");

        RuleFor(x => x.SenderUserId)
            .NotEmpty()
            .GreaterThan(0)
            .WithMessage("Valid SenderUserId is required.");
    }
}
