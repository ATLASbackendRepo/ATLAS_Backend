using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.Auth
{
    public record SaveRefreshTokenCommand(
     int UserId,
     string RefreshToken
 ) : IRequest;
}
