using FluentValidation;

namespace ATLAsBLL.Features.Users.Commands.DeleteUser
{
    public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
    {
        public DeleteUserCommandValidator()
        {
            RuleFor(u => u.UserId)
                .NotEmpty().WithMessage("User ID is required for deletion.")
                .GreaterThan(0).WithMessage("Invalid User ID.");
        }
    }
}