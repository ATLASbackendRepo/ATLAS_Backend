
using ATLAsDCL.UnitOfWork;
using MediatR;

/// <summary>
/// Get All Institutions Query.
/// </summary>
public record GetAllInstitutionsQuery() : IRequest<List<InstitutionDto>>;

/// <summary>
/// Get All Institutions Handler.
/// </summary>
public class GetAllInstitutionsHandler : IRequestHandler<GetAllInstitutionsQuery, List<InstitutionDto>>
{
    /// <summary>
    /// _unit Of Work.
    /// </summary>
    private readonly IUnitOfWork _unitOfWork;
    public GetAllInstitutionsHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<List<InstitutionDto>> Handle(GetAllInstitutionsQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Institutions.GetAllInstitutionsAsync();
    }
}


/// <summary>
/// Get Institution By Id Query.
/// </summary>
public record GetInstitutionByIdQuery(int Id) : IRequest<InstitutionDto?>;

/// <summary>
/// Get Institution By Id Handler.
/// </summary>
public class GetInstitutionByIdHandler : IRequestHandler<GetInstitutionByIdQuery, InstitutionDto?>
{
    /// <summary>
    /// unit Of Work1.
    /// </summary>
    private readonly IUnitOfWork unitOfWork1;
    public GetInstitutionByIdHandler(IUnitOfWork unitOfWork) => unitOfWork1 = unitOfWork;

    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<InstitutionDto?> Handle(GetInstitutionByIdQuery request, CancellationToken cancellationToken)
    {
        return await unitOfWork1.Institutions.GetInstitutionByIdAsync(request.Id);


    }
}
