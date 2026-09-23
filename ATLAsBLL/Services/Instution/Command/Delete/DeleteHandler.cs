using ATLAsDCL.UnitOfWork;
using MediatR;

/// <summary>
/// Delete Institution Command.
/// </summary>
public record DeleteInstitutionCommand(int Id) : IRequest<bool>;

/// <summary>
/// Delete Institution Handler.
/// </summary>
public class DeleteInstitutionHandler : IRequestHandler<DeleteInstitutionCommand, bool>
{
    /// <summary>
    /// unit Of Work.
    /// </summary>
    private readonly IUnitOfWork unitOfWork;
    public DeleteInstitutionHandler(IUnitOfWork unit) => unitOfWork = unit;

    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<bool> Handle(DeleteInstitutionCommand request, CancellationToken cancellationToken)
    {
        var IsDeletd = await unitOfWork.Institutions.DeleteInstitutionAsync(request.Id);

        return IsDeletd;
    }
}
