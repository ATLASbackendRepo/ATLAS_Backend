using FluentValidation;
using ATLAsBLL.Services.CourseGroupMessage.Command.EditCourseGroupMessage;

namespace ATLAsBLL.Services.CourseGroupMessage.Command.EditCourseGroupMessage;

public class EditCourseGroupMessageCommandValidator : AbstractValidator<EditCourseGroupMessageCommand>
{
    public EditCourseGroupMessageCommandValidator()
    {
        RuleFor(x => x.MessageId)
            .NotEmpty()
            .GreaterThan(0)
            .WithMessage("Valid MessageId is required.");

        RuleFor(x => x.NewContent)
            .NotEmpty()
            .WithMessage("New message content is required.")
            .MaximumLength(2000)
            .WithMessage("Message content cannot exceed 2000 characters.");

        RuleFor(x => x.RequestingUserId)
            .NotEmpty()
            .GreaterThan(0)
            .WithMessage("Valid RequestingUserId is required.");
    }
}
