using FluentValidation;

namespace ATLAsBLL.Features.Students.Commands.CreateStudent
{
    public class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
    {
        public CreateStudentCommandValidator()
        {
            // التحقق من بيانات المستخدم
            RuleFor(s => s.FullName).NotEmpty().WithMessage("Full name is required.");
            RuleFor(s => s.Password).NotEmpty().MinimumLength(6).WithMessage("Password must be at least 6 characters.");
            RuleFor(s => s.StudentCode).NotEmpty().MaximumLength(20).WithMessage("Student code is required and must not exceed 20 characters.");
            RuleFor(s => s.AcademicId).NotEmpty().WithMessage("Academic ID is required.");

            // التحقق من بيانات الطالب
            RuleFor(s => s.AcademicLevel).GreaterThan(0).WithMessage("Academic level is required.");
            RuleFor(s => s.DepartmentId).GreaterThan(0).WithMessage("Invalid Department ID.");
        }
    }
}
