using ATLAsDCL.UnitOfWork;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ATLAsBLL.Services.User.qeusies
{


 
    public sealed class GetUserByIdQueryHandler
        : IRequestHandler<GetUserByIdQuery, ATLASDomain.Entities.User?>
    {
        private readonly IUnitOfWork _uintofwork;

        public GetUserByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _uintofwork = unitOfWork;
        }

        public async Task<ATLASDomain.Entities.User?> Handle(
            GetUserByIdQuery request,
            CancellationToken cancellationToken)
        {
            

            if (request.UserId <= 0)
                throw new ArgumentException(
                    "User ID must be greater than zero.",
                    nameof(request.UserId));

            return await _uintofwork.Users
                .GetByUserIdAsync(request.UserId);
        }
    }
}
