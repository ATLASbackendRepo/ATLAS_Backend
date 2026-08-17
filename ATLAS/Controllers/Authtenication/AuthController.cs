using ATLAS;
using ATLAsBLL.Services.Auth;
using ATLAsBLL.Services.Auth.Queries;
using ATLASDomain.DTO.Login;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
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
        private readonly JwtOptions _jwtOptions;


        public AuthController(IMediator mediator, IOptions<JwtOptions> jwtOptions)
        {
            _mediator = mediator;
            _jwtOptions = jwtOptions.Value;
        }


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


            
            if (User == null || !User.IsActive)
                return Unauthorized("Invalid credentials");

            bool isValidPassword =
                BCrypt.Net.BCrypt.Verify(request.Password, User.Password);


     
            if (!isValidPassword)
                return Unauthorized("Invalid credentials");




                var claims = new[]
                         {
                        new Claim(ClaimTypes.NameIdentifier, User.UserId.ToString()),
                        new Claim(ClaimTypes.Name, User.FullName),
                        new Claim(ClaimTypes.Role,User.Role.ToString() ),
                        new Claim("AcademicId", User.AcademicId)
                    };
            

            var key1 = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes( "ATLAS-Super-Secret-Key-2026-For-JWT-Signing!"));

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

            // Issue NEW access token (same claims & signing settings as login)

            var claims = new[]
                     {
                        new Claim(ClaimTypes.NameIdentifier, User.UserId.ToString()),
                        new Claim(ClaimTypes.Name, User.FullName),
                        new Claim(ClaimTypes.Role,User.Role.ToString() ),
                        new Claim("AcademicId", User.AcademicId)
                    };


            var key1 = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("ATLAS-Super-Secret-Key-2026-For-JWT-Signing!"));

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

            // Rotation: replace refresh token
            var newRefreshToken = GenerateRefreshToken();
            User.RefreshTokenHash = BCrypt.Net.BCrypt.HashPassword(newRefreshToken);
            User.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
            User.RefreshTokenRevokedAt = null;

            return Ok(new TokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            var user = await _mediator.Send(new GetUserByAcademicIdQuery(request.AcdimicID));
               

            if (user == null)
                return Ok(); // Do not reveal if user exists

            bool refreshValid = BCrypt.Net.BCrypt.Verify(request.RefreshToken, user.RefreshTokenHash);
            if (!refreshValid)
                return Ok();

            user.RefreshTokenRevokedAt = DateTime.UtcNow;
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
