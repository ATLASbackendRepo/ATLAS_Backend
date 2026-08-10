using FluentValidation;

namespace ATLAsBLL.Features.Users.Commands.CreateUsersRange
{
    public class CreateUsersRangeCommandValidator : AbstractValidator<CreateUsersRangeCommand>
    {
        public CreateUsersRangeCommandValidator()
        {
            RuleFor(x => x.Users)
                .NotEmpty().WithMessage("The users list cannot be empty.");

            
            RuleForEach(x => x.Users).ChildRules(user =>
            {
                user.RuleFor(u => u.FullName)
                    .NotEmpty().WithMessage("Full name is required for all users.")
                    .MaximumLength(150).WithMessage("Full name cannot exceed 150 characters.");

                user.RuleFor(u => u.Password)
                    .NotEmpty().WithMessage("Password is required for all users.")
                    .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");
            });
        }
    }
}