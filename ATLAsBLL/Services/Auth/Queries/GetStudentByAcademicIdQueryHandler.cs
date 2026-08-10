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
    public sealed class GetStudentByAcademicIdQueryHandler
     : IRequestHandler<GetStudentByAcademicIdQuery, ATLASDomain.Entities.Student?>
    {
        private readonly IUnitOfWork _unitofwork;

        public GetStudentByAcademicIdQueryHandler(IUnitOfWork unitofwork)
        {
            _unitofwork = unitofwork;
        }

        public async Task<ATLASDomain.Entities.Student?> Handle(
            GetStudentByAcademicIdQuery request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.AcademicId))
                throw new ValidationException("Academic ID is required.");

            var academicId = request.AcademicId.Trim();

            return await _unitofwork.Students
                .GetStudentByAcademicIDAsync(academicId);
        }
    }
}
