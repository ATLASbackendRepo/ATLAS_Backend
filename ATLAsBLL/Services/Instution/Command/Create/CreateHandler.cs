using ATLAsDCL.UnitOfWork;
using ATLASDomain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.Instution.Command.Create
{
    public record CreateInstitutionCommand(CreateInstitutionDto Dto) : IRequest<InstitutionDto>;

    public class CreateInstitutionHandler : IRequestHandler<CreateInstitutionCommand, InstitutionDto>
    {
        private readonly IUnitOfWork unitOfWork;
        public CreateInstitutionHandler(IUnitOfWork unit) => unitOfWork = unit;

        public async Task<InstitutionDto> Handle(CreateInstitutionCommand request, CancellationToken cancellationToken)
        {
            return await unitOfWork.Institutions.CreateInstitutionAsync(request.Dto);
        }
    }
}
