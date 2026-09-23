using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLASDomain.DTO.Login
{
    /// <summary>
    /// Logout Request.
    /// </summary>
    public class LogoutRequest
    {
        /// <summary>
        /// Acdimic ID.
        /// </summary>
        public string? AcdimicID { get; set; }
        /// <summary>
        /// Refresh Token.
        /// </summary>
        public string? RefreshToken { get; set; }
    }
}
