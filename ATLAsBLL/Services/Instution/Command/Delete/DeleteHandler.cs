using ATLAsDCL.UnitOfWork;
using MediatR;

public record DeleteInstitutionCommand(int Id) : IRequest<bool>;

public class DeleteInstitutionHandler : IRequestHandler<DeleteInstitutionCommand, bool>
{
    private readonly IUnitOfWork unitOfWork;
    public DeleteInstitutionHandler(IUnitOfWork unit) => unitOfWork = unit;

    public async Task<bool> Handle(DeleteInstitutionCommand request, CancellationToken cancellationToken)
    {
        var IsDeletd = await unitOfWork.Institutions.DeleteInstitutionAsync(request.Id);

        return IsDeletd;
    }
}