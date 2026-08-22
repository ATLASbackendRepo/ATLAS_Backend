using ATLAsBLL.Services.profile.Staff.command;
using ATLAsBLL.Services.profile.Staff.Query;
using ATLAsBLL.Services.profile.Student.command;
using ATLAsBLL.Services.profile.Student.Query;
using ATLASDomain.DTO.Profile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ATLAS.API.Controllers.Profile
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfilesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProfilesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("STaffProfile")]
        [ProducesResponseType(typeof(StaffProfileResponseDto), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [Authorize(Roles = "Doctor,TeachingAssistant")]
        [EnableRateLimiting("ProfileReadPolicy")]


        public async Task<ActionResult<StaffProfileResponseDto>> GetByUserId()

        {
           var userId = GetCurrentUserId();

            var result = await _mediator.Send(new GetStaffProfileByUserIdQuery(userId));

            if (result == null)
                return NotFound($"No  profile found ");

            return Ok(result);
        }

  

            [HttpPut("UpdateStaffProfile")]
      
            [ProducesResponseType(StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status400BadRequest)]
            [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Doctor,TeachingAssistant")]
        [EnableRateLimiting("ProfileUpdatePolicy")]
        public async Task<IActionResult> UpdateProfile([FromBody] StaffProfileUpdateDTO dto)
            {
                var userId = GetCurrentUserId();
            var command = new updateStaffProfileInfoCommand(userId, dto);

            var result = await _mediator.Send(command);

                if (!result)
                    return NotFound(new { message = "Staff profile not found." });

                return Ok(new { message = "Profile updated successfully." });
            }

       
        [HttpGet("STudentProfile")]
        [ProducesResponseType(typeof(StaffProfileResponseDto), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [Authorize(Roles = "Student")]
        [EnableRateLimiting("ProfileReadPolicy")]

        public async Task<ActionResult<StaffProfileResponseDto>> GetStudentProfileByUserId()

        {
            var userId = GetCurrentUserId();

            var result = await _mediator.Send(new GetStudentProfileByUserIdQuery(userId));

            if (result == null)
                return NotFound($"No  profile found ");

            return Ok(result);
        }

  

            [HttpPut("UpdateStudentProfile")]
            
            [ProducesResponseType(StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status400BadRequest)]
            [ProducesResponseType(StatusCodes.Status404NotFound)]
            [Authorize(Roles = "Student")]
           [EnableRateLimiting("ProfileUpdatePolicy")]
        public async Task<IActionResult> UpdateStudentProfile([FromBody] StudentUpdateProfileInfoDTO dto)
            {
                var userId = GetCurrentUserId();
            var command = new updateStudentProfileInfoCommand(userId, dto);

            var result = await _mediator.Send(command);

                if (!result)
                    return NotFound(new { message = "Student profile not found." });

                return Ok(new { message = "Profile updated successfully." });
            }
       





        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token.");
            }
            return userId;
        }





    }
}