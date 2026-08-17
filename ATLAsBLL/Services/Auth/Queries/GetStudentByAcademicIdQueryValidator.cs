using FluentValidation;


namespace ATLAsBLL.Services.Auth.Queries
{
    public sealed class GetStudentByAcademicIdQueryValidator
      : AbstractValidator<GetUserByAcademicIdQuery>
    {
        public GetStudentByAcademicIdQueryValidator()
        {
            RuleFor(x => x.AcademicId)
                .NotEmpty().WithMessage("Academic ID is required.")
                .MaximumLength(50);
        }
    }
}
