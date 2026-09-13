using ATLAS;
using ATLAsBLL.Services;
using ATLAsBLL.Services.Auth;
using ATLAsBLL.Services.Auth.Queries;
using ATLASDomain.DTO.Login;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
using ATLASDomain.InterFaces;
using Azure.Core;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ATLAS.Controllers.Authtenication
{
    [ApiController]
    [Route("api/Auth")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ISecurityEventService _securityEventService;
        private readonly JwtOptions _jwtOptions;


        public AuthController(IMediator mediator, IOptions<JwtOptions> jwtOptions, ISecurityEventService securityEventService)
        {
            _mediator = mediator;
            _securityEventService = securityEventService;
            _jwtOptions = jwtOptions.Value;
        }


        //[HttpPost("test-discord")]
        //public async Task<IActionResult> TestDiscord()
        //{
        //    var webhookUrl = "https://discord.com/api/webhooks/1543276524961005569/DiWYEsz8u7julzUaD-vrqzqb-8i7krBymPZIA6sDCixjjL2hp7yo6Rrjd9Fh67iDGjT6";

        //    using var client = new HttpClient();

        //    var payload = new
        //    {
        //        content = "🔥 ATLAS Discord Test From ASP.NET Core"
        //    };

        //    var response = await client.PostAsJsonAsync(
        //        webhookUrl,
        //        payload);

        //    var body = await response.Content.ReadAsStringAsync();

        //    return StatusCode(
        //        (int)response.StatusCode,
        //        new
        //        {
        //            StatusCode = (int)response.StatusCode,
        //            Response = body
        //        });
        //}


        private static string GenerateRefreshToken()
        {
            var bytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }



        [HttpPost("Login")]
        [EnableRateLimiting("AuthLimiter")]
        public async Task<IActionResult> Login([FromBody]  LoginDTO request)
        {
            if (request is null ||
               string.IsNullOrWhiteSpace(request.AcademicId) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Academic ID and password are required.");
            }

            var User = await _mediator.Send(new GetUserByAcademicIdQuery(request.AcademicId));



            if (User == null || !BCrypt.Net.BCrypt.Verify(request.Password, User.Password))
            {
                var recentFailures = await _securityEventService.CountRecentFailedLoginsAsync(
                          request.AcademicId, TimeSpan.FromMinutes(9));

                var severity = recentFailures >= 5 ? "High"
                             : recentFailures >= 3 ? "Medium"
                             : "Low";

                await _securityEventService.LogAsync(new SecurityEventEntry
                {
                    EventType = "FailedLogin",
                    AttemptedAcademicId = request.AcademicId,
                    Severity = severity,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    CorrelationId = HttpContext.TraceIdentifier
                });

                return Unauthorized();
            }




                var claims = new[]
                         {
                        new Claim(ClaimTypes.NameIdentifier, User.UserId.ToString()),
                        new Claim(ClaimTypes.Name, User.FullName),
                        new Claim(ClaimTypes.Role,User.Role.ToString() ),
                        new Claim("AcademicId", User.AcademicId)
                    };
            

            var key1 = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtOptions.Key));

            var creds1 = new SigningCredentials(
                key1,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    _jwtOptions.ExpiryMinutes),
                signingCredentials: creds1
            );

            var handler = new JwtSecurityTokenHandler();
            handler.OutboundClaimTypeMap.Clear();
            var accessToken = handler.WriteToken(token);

            var refreshToken = GenerateRefreshToken();

            User.RefreshTokenHash = BCrypt.Net.BCrypt.HashPassword(refreshToken);
            User.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
            User.RefreshTokenRevokedAt = null;
            await _mediator.Send(
                new SaveRefreshTokenCommand(
                    User.UserId,
                    refreshToken
                )
            );
            return Ok(new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }



        [HttpPost("refresh")]
        [EnableRateLimiting("AuthLimiter")]
        public async Task<IActionResult> Refresh([FromBody] ATLASDomain.DTO.Login.RefreshRequest request)
        {
            if (request is null ||
                string.IsNullOrWhiteSpace(request.RefreshToken) ||
                string.IsNullOrWhiteSpace(request.AcdimicID))
            {
                return BadRequest("Refresh token and Academic ID are required.");
            }


            var User = await _mediator.Send(new GetUserByAcademicIdQuery(request.AcdimicID));


            if (User == null || !User.IsActive)
                return Unauthorized("Invalid credentials");

            if (User.RefreshTokenRevokedAt != null)
                return Unauthorized("Refresh token is revoked");

            if (User.RefreshTokenExpiresAt == null || User.RefreshTokenExpiresAt <= DateTime.UtcNow)
                return Unauthorized("Refresh token expired");

            bool refreshValid = BCrypt.Net.BCrypt.Verify(request.RefreshToken, User.RefreshTokenHash);
            if (!refreshValid)
                return Unauthorized("Invalid refresh token");

            

            var claims = new[]
                     {
                        new Claim(ClaimTypes.NameIdentifier, User.UserId.ToString()),
                        new Claim(ClaimTypes.Name, User.FullName),
                        new Claim(ClaimTypes.Role,User.Role.ToString() ),
                        new Claim("AcademicId", User.AcademicId)
                    };


            var key1 = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtOptions.Key));

            var creds1 = new SigningCredentials(
                key1,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    _jwtOptions.ExpiryMinutes),
                signingCredentials: creds1
            );
            var newAccessToken = new JwtSecurityTokenHandler().WriteToken(token);

            
            var newRefreshToken = GenerateRefreshToken();
            User.RefreshTokenHash = BCrypt.Net.BCrypt.HashPassword(newRefreshToken);
            User.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
            User.RefreshTokenRevokedAt = null;
            await _mediator.Send(new SaveRefreshTokenCommand(User.UserId, newRefreshToken));

            return Ok(new TokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            if (request is null ||
                string.IsNullOrWhiteSpace(request.RefreshToken) ||
                string.IsNullOrWhiteSpace(request.AcdimicID))
            {
                return BadRequest("Refresh token and Academic ID are required.");
            }

            var user = await _mediator.Send(new GetUserByAcademicIdQuery(request.AcdimicID));
               

            if (user == null)
                return Ok(); 

            bool refreshValid = BCrypt.Net.BCrypt.Verify(request.RefreshToken, user.RefreshTokenHash);
            if (!refreshValid)
                return Ok();

            await _mediator.Send(new RevokeRefreshTokenCommand(user.UserId));
            return Ok("Logged out successfully");
        }
























        //[HttpPost("StaffLogin")]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //public async Task<IActionResult> StaffLogin([FromBody] StaffLoginDTO request)
        //{
        //    if (request is null ||
        //       request.StaffID <= 0 ||
        //        string.IsNullOrWhiteSpace(request.Password))
        //    {
        //        return BadRequest("Staff ID and password are required.");
        //    }

        //    var staff = await _mediator.Send(new GetUserByIdQuery(request.StaffID));



        //    if (staff == null || !staff.IsActive || staff.Role is enUserRole.Student)
        //        return Unauthorized("Invalid credentials");


        //    bool isValidPassword =
        //        BCrypt.Net.BCrypt.Verify(request.Password, staff.Password);



        //    if (!isValidPassword)
        //        return Unauthorized("Invalid credentials");


        //    var claims = new[]
        //             {
        //                new Claim(ClaimTypes.NameIdentifier, staff.UserId.ToString()),
        //                new Claim(ClaimTypes.Name, staff.FullName),
        //                new Claim(ClaimTypes.Role, staff.Role.ToString()),
        //                new Claim("StaffId", staff.UserId.ToString())
        //            };


        //    var key1 = new SymmetricSecurityKey(
        //        Encoding.UTF8.GetBytes(_jwtOptions.Key));

        //    var creds1 = new SigningCredentials(
        //        key1,
        //        SecurityAlgorithms.HmacSha256);

        //    var token = new JwtSecurityToken(
        //        issuer: _jwtOptions.Issuer,
        //        audience: _jwtOptions.Audience,
        //        claims: claims,
        //        expires: DateTime.UtcNow.AddMinutes(
        //            _jwtOptions.ExpiryMinutes),
        //        signingCredentials: creds1
        //    );

        //    return Ok(new
        //    {
        //        token = new JwtSecurityTokenHandler().WriteToken(token)
        //    });
        //}





    }
}
