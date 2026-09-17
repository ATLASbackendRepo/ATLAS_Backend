
using ATLAsDCL.UnitOfWork;
using MediatR;

public record GetAllInstitutionsQuery() : IRequest<List<InstitutionDto>>;

public class GetAllInstitutionsHandler : IRequestHandler<GetAllInstitutionsQuery, List<InstitutionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetAllInstitutionsHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<List<InstitutionDto>> Handle(GetAllInstitutionsQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Institutions.GetAllInstitutionsAsync();
    }
}


public record GetInstitutionByIdQuery(int Id) : IRequest<InstitutionDto?>;

public class GetInstitutionByIdHandler : IRequestHandler<GetInstitutionByIdQuery, InstitutionDto?>
{
    private readonly IUnitOfWork unitOfWork1;
    public GetInstitutionByIdHandler(IUnitOfWork unitOfWork) => unitOfWork1 = unitOfWork;

    public async Task<InstitutionDto?> Handle(GetInstitutionByIdQuery request, CancellationToken cancellationToken)
    {
        return await unitOfWork1.Institutions.GetInstitutionByIdAsync(request.Id);


    }
}