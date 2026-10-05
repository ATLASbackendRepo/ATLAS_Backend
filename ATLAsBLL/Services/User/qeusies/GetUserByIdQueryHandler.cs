using ATLAsDCL.UnitOfWork;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ATLAsBLL.Services.User.qeusies
{


 
    /// <summary>
    /// Get User By Id Query Handler.
    /// </summary>
    public sealed class GetUserByIdQueryHandler
        : IRequestHandler<GetUserByIdQuery, ATLASDomain.Entities.User?>
    {
        /// <summary>
        /// _uintofwork.
        /// </summary>
        private readonly IUnitOfWork _uintofwork;

        public GetUserByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _uintofwork = unitOfWork;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<ATLASDomain.Entities.User?> Handle(
            GetUserByIdQuery request,
            CancellationToken cancellationToken)
        {
            

            if (request.UserId <= 0)
                throw new ArgumentException(
                    "User ID must be greater than zero.",
                    nameof(request.UserId));

            return await _uintofwork.Users
                .GetByUserIdAsync(request.UserId, cancellationToken);
        }
    }
}
