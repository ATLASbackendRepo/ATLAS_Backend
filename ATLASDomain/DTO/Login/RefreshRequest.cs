using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLASDomain.DTO.Login
{
    /// <summary>
    /// Refresh Request.
    /// </summary>
    public class RefreshRequest
    {
        /// <summary>
        /// Refresh Token.
        /// </summary>
        public string? RefreshToken { get; set; }
        /// <summary>
        /// Acdimic ID.
        /// </summary>
        public string? AcdimicID { get; set; }
    }
}
