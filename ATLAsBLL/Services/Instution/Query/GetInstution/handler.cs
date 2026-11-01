using ATLAsBLL.Exceptions;
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
    private readonly ICurrentUserService _currentUserService;

    public GetAllInstitutionsHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<List<InstitutionDto>> Handle(GetAllInstitutionsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsSAdmin)
        {
            return await _unitOfWork.Institutions.GetAllInstitutionsAsync(cancellationToken);
        }

        // Regular admin is scoped exclusively to their own institution
        if (!_currentUserService.InstitutionId.HasValue || _currentUserService.InstitutionId.Value <= 0)
        {
            throw new ForbiddenAccessException("User does not belong to a valid institution.");
        }

        var institution = await _unitOfWork.Institutions.GetInstitutionByIdAsync(_currentUserService.InstitutionId.Value, cancellationToken);
        return institution != null ? new List<InstitutionDto> { institution } : new List<InstitutionDto>();
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
    private readonly ICurrentUserService _currentUserService;

    public GetInstitutionByIdHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        unitOfWork1 = unitOfWork;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<InstitutionDto?> Handle(GetInstitutionByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsSAdmin)
        {
            if (!_currentUserService.InstitutionId.HasValue || _currentUserService.InstitutionId.Value <= 0)
            {
                throw new ForbiddenAccessException("User does not belong to a valid institution.");
            }

            if (request.Id != _currentUserService.InstitutionId.Value)
            {
                throw new ForbiddenAccessException("You do not have access to view this institution.");
            }
        }

        return await unitOfWork1.Institutions.GetInstitutionByIdAsync(request.Id, cancellationToken);
    }
}
