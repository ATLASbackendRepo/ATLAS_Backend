using ATLAsBLL.Features.Users.Commands.CreateUser;
using ATLAsBLL.Features.Users.Commands.DeleteUser;
using ATLAsBLL.Features.Users.Commands.UpdateUser;
using ATLAsBLL.Services.User.qeusies.GetallStaff;
using ATLASDomain.DTO.Staff;
using ATLASDomain.DTO.Student;
using ATLASDomain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ATLAS.Controllers.Staff
{
    /// <summary>
    /// Staff Controller.
    /// </summary>
    [Route("api/Staff")]
    [Authorize]
    public class StaffController : Controller
    {
        /// <summary>
        /// _mediator.
        /// </summary>
        private readonly IMediator _mediator;

        public StaffController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get All Student.
        /// </summary>
        [HttpGet("AllStaff", Name = "GetAllStaff")]
        [Authorize(Roles = "Admin,SAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [EnableRateLimiting("AdminGetAllPolicy")]
        public async Task<ActionResult<IEnumerable<StaffDto>>> GetAllStudent(enUserRole role , [FromQuery] StaffFilterDTO staffFilter)
        {

            var Staff = await _mediator.Send(new GetallStaff(role, staffFilter));

            // LOW-07: an empty result set is a successful query and returns 200
            // with an empty array. The 404 here meant a client could not tell
            // "this role has no staff" apart from "the endpoint is gone", and the
            // role is required, so the common case of filtering to an empty role
            // was reported as a missing resource.
            return Ok(Staff ?? new List<StaffDto>());
        }

        /// <summary>
        /// Create Staff.
        /// </summary>
        [HttpPost("StaffStudent")]
        [Authorize(Roles = "SAdmin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [EnableRateLimiting("AdminUpdatePolicy")]
        public async Task<ActionResult> CreateStaff([FromBody] CreateStaffDto createStaffDTO)
        {
                var staffId = await _mediator.Send(new CreateUserCommand(createStaffDTO));
                if (staffId > 0)
                {
                    // LOW-07: creating a staff member is a 201, not a 200.
                    return StatusCode(StatusCodes.Status201Created, new { message = "Staff created successfully.", StaffId = staffId });
                }
                else
                {
                    return BadRequest("Failed to create staff.");
                }
        }


        /// <summary>
        /// update Staff.
        /// </summary>
        [HttpPut("UpdateStaff")]
        [Authorize(Roles = "SAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [EnableRateLimiting("AdminUpdatePolicy")]
        public async Task<ActionResult> updateStaff([FromBody] Create_Update_StaffDto UpdateStaffDTO)
        {
            
                var staff = await _mediator.Send(new UpdateUserCommand(UpdateStaffDTO));
                if (staff)
                {
                    return Ok(new { message = "Staff updated successfully." });
                }
                else
                {
                    return BadRequest("Failed to update staff.");
                }
           
        }

        /// <summary>
        /// delete Staff.
        /// </summary>
        [HttpDelete("DeleteStaff")]
        [Authorize(Roles = "SAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [EnableRateLimiting("AdminSoftDeletePolicy")]
        public async Task<ActionResult> deleteStaff([FromQuery] string StaffCode)
        {
            
                var staff = await _mediator.Send(new DeleteUserCommand(StaffCode));
                if (staff)
                {
                    return Ok(new { message = "Staff deleted successfully." });
                }
                else
                {
                    return BadRequest("Failed to delete staff.");
                }
          
        }

    }
}
