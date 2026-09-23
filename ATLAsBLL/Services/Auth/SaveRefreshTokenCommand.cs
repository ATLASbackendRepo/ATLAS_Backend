using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.Auth
{
    /// <summary>
    /// Save Refresh Token Command.
    /// </summary>
    public record SaveRefreshTokenCommand(
     int UserId,
     string RefreshToken
 ) : IRequest;
}
