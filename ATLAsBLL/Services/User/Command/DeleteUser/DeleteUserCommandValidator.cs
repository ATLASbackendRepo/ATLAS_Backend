using FluentValidation;

namespace ATLAsBLL.Features.Users.Commands.DeleteUser
{
    /// <summary>
    /// Delete User Command Validator.
    /// </summary>
    public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
    {
        public DeleteUserCommandValidator()
        {
            RuleFor(u => u.AcademicId)
                .NotEmpty().WithMessage("User ID is required for deletion.")
                .MaximumLength(150).WithMessage("Invalid User ID.");
        }
    }
}
