using ATLAsDCL.UnitOfWork;
using MediatR;

/// <summary>
/// Update Institution Command.
/// </summary>
public record UpdateInstitutionCommand(UpdateInstitutionDto Dto) : IRequest<bool>;

/// <summary>
/// Update Institution Handler.
/// </summary>
public class UpdateInstitutionHandler : IRequestHandler<UpdateInstitutionCommand, bool>
{
    /// <summary>
    /// unit Of Work.
    /// </summary>
    private readonly IUnitOfWork unitOfWork;
    public UpdateInstitutionHandler(IUnitOfWork unit) => unitOfWork = unit;

    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<bool> Handle(UpdateInstitutionCommand request, CancellationToken cancellationToken)
    {
        var ISupdated = await unitOfWork.Institutions.UpdateInstitutionAsync(request.Dto);

        return ISupdated;
    }
}
