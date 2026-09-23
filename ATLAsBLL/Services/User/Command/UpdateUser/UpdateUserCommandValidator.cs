using FluentValidation;

namespace ATLAsBLL.Features.Users.Commands.UpdateUser
{
    /// <summary>
    /// Update User Command Validator.
    /// </summary>
    public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator()
        {
     
            RuleFor(u => u.Update_StaffDto.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(150).WithMessage("Full name cannot exceed 150 characters.");

        }
    }
}
