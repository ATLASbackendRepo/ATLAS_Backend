using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLASDomain.DTO.Login
{
    public class LogoutRequest
    {
        public string? AcdimicID { get; set; }
        public string? RefreshToken { get; set; }
    }
}
