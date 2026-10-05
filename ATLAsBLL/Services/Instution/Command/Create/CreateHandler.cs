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
    /// <summary>
    /// Create Institution Command.
    /// </summary>
    public record CreateInstitutionCommand(CreateInstitutionDto Dto) : IRequest<InstitutionDto>;

    /// <summary>
    /// Create Institution Handler.
    /// </summary>
    public class CreateInstitutionHandler : IRequestHandler<CreateInstitutionCommand, InstitutionDto>
    {
        /// <summary>
        /// unit Of Work.
        /// </summary>
        private readonly IUnitOfWork unitOfWork;
        public CreateInstitutionHandler(IUnitOfWork unit) => unitOfWork = unit;

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<InstitutionDto> Handle(CreateInstitutionCommand request, CancellationToken cancellationToken)
        {
            return await unitOfWork.Institutions.CreateInstitutionAsync(request.Dto, cancellationToken);
        }
    }
}
