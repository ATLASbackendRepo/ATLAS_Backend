using ATLASDomain.DTO.Login;
using ATLASDomain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.User.Command.ChangePassword
{
    /// <summary>
    /// Change Passwordcommand.
    /// </summary>
    public record ChangePasswordcommand(ATLASDomain.Entities.User user ,ATLASDomain.DTO.Login.ChangePassword model ) : IRequest<bool>
    {
    }
}
