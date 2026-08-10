using FluentValidation;

namespace ATLAsBLL.Features.Students.Commands.CreateStudentsRange
{
    public class CreateStudentsRangeCommandValidator : AbstractValidator<CreateStudentsRangeCommand>
    {
        public CreateStudentsRangeCommandValidator()
        {
            RuleFor(x => x.Students)
                .NotEmpty().WithMessage("The students list cannot be empty.");

       
            RuleForEach(x => x.Students).ChildRules(student =>
            {
               
                student.RuleFor(s => s.FullName)
                    .NotEmpty().WithMessage("Full name is required for all students.")
                    .MaximumLength(150).WithMessage("Full name cannot exceed 150 characters.");

                student.RuleFor(s => s.Password)
                    .NotEmpty().WithMessage("Password is required for all students.")
                    .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");

              
                student.RuleFor(s => s.StudentCode)
                    .NotEmpty().WithMessage("Student code is required.")
                    .MaximumLength(50).WithMessage("Student code cannot exceed 50 characters.");

                student.RuleFor(s => s.AcademicId)
                    .NotEmpty().WithMessage("Academic ID is required.");

                student.RuleFor(s => s.AcademicLevel)
                    .NotEmpty().WithMessage("Academic level is required.");

                student.RuleFor(s => s.DepartmentId)
                    .NotEmpty().WithMessage("Department ID is required.")
                    .GreaterThan(0).WithMessage("Invalid Department ID.");
            });
        }
    }
}
