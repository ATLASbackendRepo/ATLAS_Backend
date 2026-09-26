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
    [Route("api/Staff")]
    [Authorize]
    /// <summary>
    /// Staff Controller.
    /// </summary>
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

        [HttpGet("AllStaff", Name = "GetAllStaff")]
        [Authorize(Roles = "Admin,SAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [EnableRateLimiting("AdminGetAllPolicy")]
        /// <summary>
        /// Get All Student.
        /// </summary>
        public async Task<ActionResult<IEnumerable<StaffDto>>> GetAllStudent(enUserRole role , [FromQuery] StaffFilterDTO staffFilter)
        {

            var Staff = await _mediator.Send(new GetallStaff(role, staffFilter));
            if (Staff == null || (Staff.Count() == 0))
            {
                return NotFound("No staff found.");
            }
            return Ok(Staff);
        }

        [HttpPost("StaffStudent")]
        [Authorize(Roles = "SAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [EnableRateLimiting("AdminUpdatePolicy")]
        /// <summary>
        /// Create Staff.
        /// </summary>
        public async Task<ActionResult> CreateStaff([FromBody] CreateStaffDto createStaffDTO)
        {
                var staffId = await _mediator.Send(new CreateUserCommand(createStaffDTO));
                if (staffId > 0)
                {
                    return Ok(new { message = "Staff created successfully.", StaffId = staffId });
                }
                else
                {
                    return BadRequest("Failed to create staff.");
                }
        }


        [HttpPut("UpdateStaff")]
        [Authorize(Roles = "SAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [EnableRateLimiting("AdminUpdatePolicy")]
        /// <summary>
        /// update Staff.
        /// </summary>
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

        [HttpDelete("DeleteStaff")]
        [Authorize(Roles = "SAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [EnableRateLimiting("AdminSoftDeletePolicy")]
        /// <summary>
        /// delete Staff.
        /// </summary>
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
