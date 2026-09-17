using ATLAsDCL.UnitOfWork;
using MediatR;

public record UpdateInstitutionCommand(UpdateInstitutionDto Dto) : IRequest<bool>;

public class UpdateInstitutionHandler : IRequestHandler<UpdateInstitutionCommand, bool>
{
    private readonly IUnitOfWork unitOfWork;
    public UpdateInstitutionHandler(IUnitOfWork unit) => unitOfWork = unit;

    public async Task<bool> Handle(UpdateInstitutionCommand request, CancellationToken cancellationToken)
    {
        var ISupdated = await unitOfWork.Institutions.UpdateInstitutionAsync(request.Dto);

        return ISupdated;
    }
}
