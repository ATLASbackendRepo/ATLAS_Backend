using ATLAS;
using ATLAsBLL.Services;
using ATLAsBLL.Services.Auth;
using ATLAsBLL.Services.Auth.Queries;
using ATLAsBLL.Services.User.Command.ChangePassword;
using ATLAsBLL.Services.User.qeusies;
using ATLASDomain.DTO.Login;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
using ATLASDomain.InterFaces;
using Azure.Core;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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
    /// <summary>
    /// Auth Controller.
    /// </summary>
    [ApiController]
    [Route("api/Auth")]
 
    public class AuthController : ControllerBase
    {
        /// <summary>
        /// _mediator.
        /// </summary>
        private readonly IMediator _mediator;
        /// <summary>
        /// _security Event Service.
        /// </summary>
        private readonly ISecurityEventService _securityEventService;
        /// <summary>
        /// _jwt Options.
        /// </summary>
        private readonly JwtOptions _jwtOptions;


        public AuthController(IMediator mediator, IOptions<JwtOptions> jwtOptions, ISecurityEventService securityEventService)
        {
            _mediator = mediator;
            _securityEventService = securityEventService;
            _jwtOptions = jwtOptions.Value;
        }


        /// <summary>
        /// Generate Refresh Token.
        /// </summary>
        private static string GenerateRefreshToken()
        {
            var bytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }



        /// <summary>
        /// Login.
        /// </summary>
        [HttpPost("Login")]
        [EnableRateLimiting("AuthLimiter")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody]  LoginDTO request)
        {
            if (request is null ||
               string.IsNullOrWhiteSpace(request.AcademicId) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Academic ID and password are required.");
            }

            var User = await _mediator.Send(new GetUserByAcademicIdQuery(request.AcademicId));



            if (User == null || !User.IsActive || !BCrypt.Net.BCrypt.Verify(request.Password, User.Password))
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

            if (User.MustChangePassword)
                return StatusCode(StatusCodes.Status428PreconditionRequired, new { message = "Password change is required before signing in." });




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



        /// <summary>
        /// Refresh.
        /// </summary>
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
                return Unauthorized();

            if (User.RefreshTokenRevokedAt != null)
                return Unauthorized();

            if (User.RefreshTokenExpiresAt == null || User.RefreshTokenExpiresAt <= DateTime.UtcNow)
                return Unauthorized();

            bool currentMatches = User.RefreshTokenHash != null
                && BCrypt.Net.BCrypt.Verify(request.RefreshToken, User.RefreshTokenHash);

            bool previousMatches = User.PreviousRefreshTokenHash != null
                && BCrypt.Net.BCrypt.Verify(request.RefreshToken, User.PreviousRefreshTokenHash);

            if (!currentMatches && !previousMatches)
                return Unauthorized();

            // Reuse detection: a token that was already rotated must never be
            // accepted again. If it is, the whole family is revoked immediately.
            if (!currentMatches && previousMatches)
            {
                await _mediator.Send(new RevokeRefreshTokenCommand(User.UserId));
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
            var newAccessToken = new JwtSecurityTokenHandler().WriteToken(token);

            
            var newRefreshToken = GenerateRefreshToken();
            await _mediator.Send(new SaveRefreshTokenCommand(User.UserId, newRefreshToken));

            return Ok(new TokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }

        /// <summary>
        /// Logout.
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
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






        /// <summary>
        /// Change Password.
        /// </summary>
        [HttpPost("ChangePassword")]
        [Authorize]
        [EnableRateLimiting("AuthLimiter")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePassword model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 2. Prevent using the same password
            if (model.OldPassword == model.NewPassword)
            {
                return BadRequest(new { message = "New password must be different from the old password." });
            }

            var AcademicId = GetCurrentACdimicId();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = !string.IsNullOrEmpty(userId) && int.TryParse(userId, out var parsedUserId)
                ? await _mediator.Send(new GetUserByIdQuery(parsedUserId))
                : !string.IsNullOrWhiteSpace(AcademicId)
                    ? await _mediator.Send(new GetUserByAcademicIdQuery(AcademicId))
                    : null;
            if (user == null)
            {
                return Unauthorized();
            }

            // 5. Change Password via UserManager
            bool isChange;
            try
            {
                isChange = await _mediator.Send(new ChangePasswordcommand(user, model));
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (InvalidDataException exception)
            {
                return BadRequest(new { message = exception.Message });
            }

            // 6. Check Output Result
            if (!isChange)
            {
                return BadRequest("Password change failed." );
            }

            return Ok(new { message = "Password changed successfully." });
        }

        /// <summary>
        /// Get Current ACdimic Id.
        /// </summary>
        private string GetCurrentACdimicId()
        {
            var userIdClaim = User.FindFirst("AcademicId");

            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                throw new UnauthorizedAccessException("User ID not found in token.");
            }

            return userIdClaim.Value;
        }



















    }
}
