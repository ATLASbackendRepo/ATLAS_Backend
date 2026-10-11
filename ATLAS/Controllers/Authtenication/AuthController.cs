using ATLAS;
using ATLAS.Claims;
using ATLAsBLL.Services;
using ATLAsBLL.Services.Auth;
using ATLAsBLL.Services.Auth.Queries;
using ATLAsBLL.Services.User.Command.ChangePassword;
using ATLAsBLL.Services.User.Command.AdminResetUserPassword;
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

        /// <summary>
        /// Fixed dummy hash used to equalize BCrypt timing when an account is not found (HIGH-02).
        /// </summary>
        private static readonly string DummyHash = BCrypt.Net.BCrypt.HashPassword("DummyPasswordToEqualizeTiming123!");


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
        public async Task<IActionResult> Login([FromBody]  LoginDTO request, CancellationToken cancellationToken = default)
        {
            if (request is null ||
               string.IsNullOrWhiteSpace(request.AcademicId) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Academic ID and password are required.");
            }

            var rawIp = HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var clientIp = string.IsNullOrWhiteSpace(rawIp) ? "unknown" : rawIp;
            var recentFailuresBefore = _securityEventService != null
                ? await _securityEventService.CountRecentFailedLoginsAsync(request.AcademicId, clientIp, TimeSpan.FromMinutes(9), cancellationToken)
                : 0;

            if (recentFailuresBefore >= 5)
            {
                if (_securityEventService != null)
                {
                    await _securityEventService.LogAsync(new SecurityEventEntry
                    {
                        EventType = "AccountLockout",
                        AttemptedAcademicId = request.AcademicId,
                        Severity = "High",
                        IpAddress = clientIp,
                        CorrelationId = HttpContext?.TraceIdentifier,
                        Description = "Too many failed login attempts; account temporarily locked."
                    }, cancellationToken);
                }

                return StatusCode(StatusCodes.Status429TooManyRequests, new
                {
                    message = "Too many failed login attempts. Please try again later."
                });
            }

            var User = await _mediator.Send(new GetUserByAcademicIdQuery(request.AcademicId), cancellationToken);

            var isUserValid = User != null && User.IsActive && !string.IsNullOrEmpty(User.Password);
            var hashToVerify = isUserValid ? User!.Password : DummyHash;
            var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, hashToVerify);

            if (!isUserValid || !isPasswordValid)
            {
                var recentFailures = recentFailuresBefore + 1;

                var severity = recentFailures >= 5 ? "High"
                             : recentFailures >= 3 ? "Medium"
                             : "Low";

                if (_securityEventService != null)
                {
                    await _securityEventService.LogAsync(new SecurityEventEntry
                    {
                        EventType = "FailedLogin",
                        AttemptedAcademicId = request.AcademicId,
                        Severity = severity,
                        IpAddress = clientIp,
                        CorrelationId = HttpContext?.TraceIdentifier
                    }, cancellationToken);

                    // Layered defense: Check account-wide failures across all IPs to detect distributed attacks
                    var accountWideFailures = await _securityEventService.CountRecentFailedLoginsAsync(request.AcademicId, TimeSpan.FromMinutes(15), cancellationToken);
                    if (accountWideFailures >= 10)
                    {
                        await _securityEventService.LogAsync(new SecurityEventEntry
                        {
                            EventType = "DistributedBruteForceDetected",
                            AttemptedAcademicId = request.AcademicId,
                            Severity = "High",
                            IpAddress = clientIp,
                            CorrelationId = HttpContext?.TraceIdentifier,
                            Description = $"Potential distributed brute force attack detected against academic ID '{request.AcademicId}' ({accountWideFailures} attempts across multiple sources within 15 minutes)."
                        }, cancellationToken);
                    }
                }

                return Unauthorized();
            }

            if (User.MustChangePassword)
            {
                // The account still holds the one-time password it was created
                // with. Only the password-change endpoints are reachable until
                // the mandatory change is completed, and no refresh token is
                // issued for this restricted token.
                return StatusCode(
                    StatusCodes.Status428PreconditionRequired,
                    new
                    {
                        message = "Password change is required before signing in.",
                        accessToken = CreatePasswordChangeToken(User),
                        refreshToken = (string?)null,
                        expiresInMinutes = PasswordChangeTokenMinutes
                    });
            }




                var claims = new[]
                         {
                        new Claim(ClaimTypes.NameIdentifier, User.UserId.ToString()),
                        new Claim(ClaimTypes.Name, User.FullName),
                        new Claim(ClaimTypes.Role,User.Role.ToString() ),
                        new Claim("AcademicId", User.AcademicId),

                        // MED-01: the stamp is compared against the stored value on
                        // every request. Logging out, deactivating the account or
                        // changing its role rotates the stamp, which invalidates this
                        // token immediately instead of leaving it usable until it
                        // expires.
                        new Claim(AtlasClaims.SecurityStamp, User.SecurityStamp.ToString()),

                        // MED-16: carries the institution the account belongs to so
                        // tenant-scoped queries and authorization can use it.
                        new Claim(AtlasClaims.InstitutionId,
                            User.InstitutionID.ToString())
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

            // MED-02: the login path must not also write the hash here. Doing it
            // in the controller and again in the handler hashed the token twice and
            // the second write clobbered the value the handler had just stored, which
            // is what destroyed reuse detection. SaveRefreshTokenCommand is the
            // single writer for this row.
            await _mediator.Send(
                new SaveRefreshTokenCommand(
                    User.UserId,
                    refreshToken
                )
            , cancellationToken);
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
        public async Task<IActionResult> Refresh([FromBody] ATLASDomain.DTO.Login.RefreshRequest request, CancellationToken cancellationToken = default)
        {
            if (request is null ||
                string.IsNullOrWhiteSpace(request.RefreshToken) ||
                string.IsNullOrWhiteSpace(request.AcdimicID))
            {
                return BadRequest("Refresh token and Academic ID are required.");
            }


            var User = await _mediator.Send(new GetUserByAcademicIdQuery(request.AcdimicID), cancellationToken);


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
                await _mediator.Send(new RevokeRefreshTokenCommand(User.UserId), cancellationToken);
                return Unauthorized();
            }

            // MED-02: rotate with a compare-and-swap on the hash this request was
            // issued against. Two concurrent refreshes carrying the same token both
            // pass the checks above, but only one of them can match the stored hash
            // inside the UPDATE, so only one caller receives a usable token pair.
            var newRefreshToken = GenerateRefreshToken();

            var rotation = await _mediator.Send(new RotateRefreshTokenCommand(
                User.UserId,
                // The hash observed a moment ago. Capping the rotation on this exact
                // value is what turns a read-then-write into an atomic swap.
                User.RefreshTokenHash!,
                newRefreshToken), cancellationToken);

            if (!rotation.Succeeded)
            {
                // The token was rotated away between the read above and the write.
                // Somebody is replaying a spent token, so the entire family is
                // treated as compromised rather than merely expired.
                await _mediator.Send(new RevokeRefreshTokenCommand(User.UserId), cancellationToken);
                return Unauthorized();
            }

            var claims = new[]
                     {
                        new Claim(ClaimTypes.NameIdentifier, User.UserId.ToString()),
                        new Claim(ClaimTypes.Name, User.FullName),
                        new Claim(ClaimTypes.Role,User.Role.ToString() ),
                        new Claim("AcademicId", User.AcademicId),

                        // MED-01: the refreshed token carries the stamp the account
                        // holds right now, so it stays valid across a refresh and is
                        // invalidated the moment the stamp is rotated.
                        new Claim(AtlasClaims.SecurityStamp, User.SecurityStamp.ToString()),

                        // MED-16: keeps the tenant claim present on refreshed tokens
                        // as well, so it cannot be lost by refreshing.
                        new Claim(AtlasClaims.InstitutionId,
                            User.InstitutionID.ToString())
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

            // The outbound claim map is cleared for the same reason as at login:
            // the framework rewrites long URIs into XML element names by default,
            // which would corrupt the security stamp and the institution claim.
            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.OutboundClaimTypeMap.Clear();
            var newAccessToken = tokenHandler.WriteToken(token);

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
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken = default)
        {
            if (request is null ||
                string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest("A refresh token is required.");
            }

            // The account that is being signed out is always the caller. Taking
            // the identity from the request body would let anyone holding a
            // valid access token revoke somebody else's refresh-token family.
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var callerUserId) ||
                callerUserId <= 0)
            {
                return Unauthorized();
            }

            var user = await _mediator.Send(new GetUserByIdQuery(callerUserId), cancellationToken);

            if (user == null || string.IsNullOrWhiteSpace(user.RefreshTokenHash))
                return Ok();

            // The token is only accepted when it really belongs to the
            // signed-in account, so logout can never target another user.
            bool refreshValid = BCrypt.Net.BCrypt.Verify(request.RefreshToken, user.RefreshTokenHash);
            if (!refreshValid)
                return BadRequest("The refresh token does not belong to the signed-in account.");

            await _mediator.Send(new RevokeRefreshTokenCommand(user.UserId), cancellationToken);
            return Ok("Logged out successfully");
        }






        /// <summary>
        /// Change Password.
        /// </summary>
        [HttpPost("ChangePassword")]
        [Authorize]
        [EnableRateLimiting("AuthLimiter")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePassword model, CancellationToken cancellationToken = default)
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
                ? await _mediator.Send(new GetUserByIdQuery(parsedUserId), cancellationToken)
                : !string.IsNullOrWhiteSpace(AcademicId)
                    ? await _mediator.Send(new GetUserByAcademicIdQuery(AcademicId), cancellationToken)
                    : null;
            if (user == null)
            {
                return Unauthorized();
            }

            // 5. Change Password via UserManager
            bool isChange;
            try
            {
                isChange = await _mediator.Send(new ChangePasswordcommand(user, model), cancellationToken);
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
        /// Resets the password of a target user account (Admin / SAdmin only).
        /// </summary>
        [HttpPost("users/{academicId}/reset-password")]
        [Authorize(Roles = "SAdmin,Admin")]
        [EnableRateLimiting("AuthLimiter")]
        public async Task<IActionResult> ResetUserPassword(
            [FromRoute] string academicId,
            [FromBody] ChangePasswordAdmin model,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var performingUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(performingUserIdStr, out var performingUserId))
            {
                return Unauthorized();
            }

            var roleClaim = User.FindFirstValue(ClaimTypes.Role);
            if (!Enum.TryParse<enUserRole>(roleClaim, true, out var performingRole))
            {
                return Unauthorized();
            }

            int? institutionId = null;
            var instClaim = User.FindFirstValue("institution_id");
            if (!string.IsNullOrWhiteSpace(instClaim) && int.TryParse(instClaim, out var parsedInst))
            {
                institutionId = parsedInst;
            }

            try
            {
                var success = await _mediator.Send(new AdminResetUserPasswordCommand(
                    academicId,
                    model.NewPassword,
                    model.ConfirmPassword,
                    performingUserId,
                    performingRole,
                    institutionId
                ), cancellationToken);

                if (!success)
                {
                    return BadRequest("Password reset failed.");
                }

                return Ok(new { message = "User password reset successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }



        /// <summary>
        /// Lifetime of the restricted token issued for the mandatory first
        /// password change.
        /// </summary>
        private const int PasswordChangeTokenMinutes = 10;

        /// <summary>
        /// Creates the short-lived token that is only accepted by the
        /// password-change endpoints. It carries the caller's identity plus the
        /// <see cref="AtlasClaims.PasswordChangeOnly"/> marker, and it is never
        /// paired with a refresh token.
        /// </summary>
        /// <param name="user">Account that must change its password.</param>
        /// <returns>The signed restricted JWT.</returns>
        private string CreatePasswordChangeToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(AtlasClaims.AcademicId, user.AcademicId),
                new Claim(AtlasClaims.PasswordChangeOnly, "true")
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtOptions.Key));

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(PasswordChangeTokenMinutes),
                signingCredentials: new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256));

            var handler = new JwtSecurityTokenHandler();
            handler.OutboundClaimTypeMap.Clear();

            return handler.WriteToken(token);
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
