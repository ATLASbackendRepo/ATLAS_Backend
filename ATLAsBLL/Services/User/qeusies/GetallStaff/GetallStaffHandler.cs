using ATLAsDCL.UnitOfWork;
using ATLASDomain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.User.qeusies.GetallStaff
{
    public sealed class GetallStaffHandler : IRequestHandler<GetallStaff, IEnumerable<ATLASDomain.DTO.Staff.StaffDto>>
    {
        private readonly IUnitOfWork _uintofwork;

        public GetallStaffHandler(IUnitOfWork unitOfWork)
        {
            _uintofwork = unitOfWork;
        }
        public async Task<IEnumerable<ATLASDomain.DTO.Staff.StaffDto>> Handle(GetallStaff request, CancellationToken cancellationToken)
        {
            if (request== null)
                throw new ArgumentException("Invalid role. Only Staff role is allowed.", nameof(request.role));
            var staffList = await _uintofwork.Users.GetAllStaffAsync(request.role, request.filter);
            return staffList;
        }
    }
}
