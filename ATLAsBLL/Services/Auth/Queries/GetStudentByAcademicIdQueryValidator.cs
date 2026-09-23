using FluentValidation;


namespace ATLAsBLL.Services.Auth.Queries
{
    /// <summary>
    /// Get Student By Academic Id Query Validator.
    /// </summary>
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
