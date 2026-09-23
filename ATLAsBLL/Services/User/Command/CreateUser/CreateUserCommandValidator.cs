using FluentValidation;

namespace ATLAsBLL.Features.Users.Commands.CreateUser
{
    /// <summary>
    /// Create User Command Validator.
    /// </summary>
    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(u => u.create_StaffDto.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters.");

            
            

            
    }
}
}
