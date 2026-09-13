using ATLAsBLL.Features.Students.Commands.CreateStudent;
using ATLAsBLL.Features.Students.Commands.CreateStudentsRange;
using ATLAsBLL.Features.Students.Commands.DeleteStudent;
using ATLAsBLL.Features.Students.Commands.UpdateStudent;
using ATLAsBLL.Features.Users.Commands.CreateUser;
using ATLAsBLL.Features.Users.Commands.DeleteUser;
using ATLAsBLL.Features.Users.Commands.UpdateUser;
using ATLAsBLL.Services.Student.Queries.GetallStudent;
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
    public class StaffController : Controller
    {
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
        public async Task<ActionResult> CreateStaff([FromBody] Create_Update_StaffDto createStaffDTO)
        {
            try
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
            catch (KeyNotFoundException)
            {
                return NotFound("One or more required entities not found.");
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred while creating the staff");
            }
        }


        [HttpPut("UpdateStaff")]
        [Authorize(Roles = "SAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [EnableRateLimiting("AdminUpdatePolicy")]
        public async Task<ActionResult> updateStaff([FromBody] Create_Update_StaffDto UpdateStaffDTO)
        {
            try
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
            catch(KeyNotFoundException)
            {
                return NotFound("One or more required entities not found.");
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred while updating the staff");
            }
        }

        [HttpDelete("DeleteStaff")]
        [Authorize(Roles = "SAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [EnableRateLimiting("AdminSoftDeletePolicy")]
        public async Task<ActionResult> deleteStaff([FromQuery] string StaffCode)
        {
            try
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
            catch(KeyNotFoundException)
            {
                return NotFound("One or more required entities not found.");
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred while deleting the staff");
            }
        }

    }
}
