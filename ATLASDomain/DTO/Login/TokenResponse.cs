using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLASDomain.DTO.Login
{
    /// <summary>
    /// Token Response.
    /// </summary>
    public class TokenResponse
    {
        /// <summary>
        /// Access Token.
        /// </summary>
        public string? AccessToken { get; set; }
        /// <summary>
        /// Refresh Token.
        /// </summary>
        public string?   RefreshToken { get; set; }
    }
}
