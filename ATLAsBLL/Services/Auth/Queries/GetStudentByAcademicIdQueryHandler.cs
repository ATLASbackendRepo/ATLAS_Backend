using ATLAsDCL.Data;
using ATLAsDCL.UnitOfWork;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.Auth.Queries
{
    public sealed class GetUserByAcademicIdQueryHandler
     : IRequestHandler<GetUserByAcademicIdQuery, ATLASDomain.Entities.User?>
    {
        private readonly IUnitOfWork _unitofwork;

        public GetUserByAcademicIdQueryHandler(IUnitOfWork unitofwork)
        {
            _unitofwork = unitofwork;
        }

        public async Task<ATLASDomain.Entities.User?> Handle(
            GetUserByAcademicIdQuery request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.AcademicId))
                throw new ValidationException("Academic ID is required.");

            var academicId = request.AcademicId.Trim();

            return await _unitofwork.Users
                .GetUserByAcademicIDAsync(academicId);
        }
    }
}
