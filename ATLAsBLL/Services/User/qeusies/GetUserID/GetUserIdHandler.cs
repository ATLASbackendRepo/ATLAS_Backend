using ATLAsDCL.UnitOfWork;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.User.qeusies.GetUserID
{
    public class GetUserIdHandler :IRequestHandler<GetUserID,int?>
    {
        private readonly IUnitOfWork _unitofWork; 
        public GetUserIdHandler(IUnitOfWork unitofWork)
        {
            _unitofWork = unitofWork;
        }

        public async Task<int?> Handle(GetUserID request, CancellationToken cancellationToken)
        {
            var userID = await _unitofWork.Users.GetUserIdByAcdimicId(request.AcdimicID, cancellationToken);
            return userID;
        }


    }
}
