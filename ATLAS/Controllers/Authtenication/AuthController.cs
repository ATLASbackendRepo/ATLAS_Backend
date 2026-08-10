using ATLAsBLL.Services.Auth.Queries;
using ATLAsBLL.Services.User.qeusies;
using ATLASDomain.DTO.Login;
using ATLASDomain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using ATLAS;

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

        [HttpPost("StudentLogin")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> StudentLogin([FromBody] StudentLoginDTO request)
        {
            if (request is null ||
               string.IsNullOrWhiteSpace(request.AcademicId) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Academic ID and password are required.");
            }

            var student = await _mediator.Send(new GetStudentByAcademicIdQuery(request.AcademicId));


            
            if (student == null)
                return Unauthorized("Invalid credentials");

            if (student.User is null || !student.IsActive || !student.User.IsActive)
                return Unauthorized("Invalid credentials");

            bool isValidPassword =
                BCrypt.Net.BCrypt.Verify(request.Password, student.User.Password);


     
            if (!isValidPassword)
                return Unauthorized("Invalid credentials");


            var claims = new[]
                     {
                        new Claim(ClaimTypes.NameIdentifier, student.UserId.ToString()),
                        new Claim("UserId", student.UserId.ToString()),
                        new Claim("StudentId", student.StudentId.ToString()),
                        new Claim(ClaimTypes.Name, student.User.FullName),
                        new Claim(ClaimTypes.Role,enUserRole.Student.ToString() ),
                        new Claim("AcademicId", student.AcademicId),
                        new Claim("StudentCode", student.StudentCode),
                        new Claim("Level", student.AcademicLevel.ToString())
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

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }
       
        
        
        
        [HttpPost("StaffLogin")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> StaffLogin([FromBody] StaffLoginDTO request)
        {
            if (request is null ||
               request.StaffID <= 0 ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Staff ID and password are required.");
            }

            var staff = await _mediator.Send(new GetUserByIdQuery(request.StaffID));


            
            if (staff == null || !staff.IsActive || staff.Role is enUserRole.Student)
                return Unauthorized("Invalid credentials");


            bool isValidPassword =
                BCrypt.Net.BCrypt.Verify(request.Password, staff.Password);


     
            if (!isValidPassword)
                return Unauthorized("Invalid credentials");


            var claims = new[]
                     {
                        new Claim(ClaimTypes.NameIdentifier, staff.UserId.ToString()),
                        new Claim(ClaimTypes.Name, staff.FullName),
                        new Claim(ClaimTypes.Role, staff.Role.ToString()),
                        new Claim("StaffId", staff.UserId.ToString())
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

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }





    }
}
