using ATLAsBLL.Features.Department.Comand.CreateDepartment;
using ATLAsBLL.Features.Department.Comand.DeleteDepartment;
using ATLAsBLL.Features.Department.Comand.UpdateDepartment;
using ATLAsBLL.Services.Department.Query;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ATLAS.Controllers.Department
{
    /// <summary>
    /// Department Controller.
    /// </summary>
    [Route("api/Department")]
    [ApiController]
    public class DepartmentController : Controller
    {

        private readonly IMediator _mediator;
        public DepartmentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create Department.
        /// </summary>
        [HttpPost("CreateDepartment")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "SAdmin,Admin")]
        [EnableRateLimiting("AdminUpdatePolicy")]
        public async Task<IActionResult> CreateDepartment([FromBody] string departmentName, int institutionId)
        {
            if (!string.IsNullOrEmpty(departmentName) && institutionId > 0)
            {
                var departmentId = await _mediator.Send(new CreateDepartmentCommand(departmentName, institutionId));
                if (departmentId <= 0)
                {
                    return BadRequest("Failed to create department.");
                }
                return Ok("Department created successfully.");
            }
            return BadRequest();
        }
        /// <summary>
        /// Update Department.
        /// </summary>
        [HttpPut("UpdateDepartment")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "SAdmin,Admin")]
        [EnableRateLimiting("AdminUpdatePolicy")]
        public async Task<IActionResult> UpdateDepartment(int departmentId, [FromBody] string departmentName)
        {
            try
            {
                if (departmentId > 0 && !string.IsNullOrEmpty(departmentName))
                {
                    var result = await _mediator.Send(new UpdateDepartmentCommand(departmentId, departmentName));
                    if (result)
                        return Ok("Department updated successfully.");

                }
                return BadRequest();
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Department not found.");
            }
        }
        /// <summary>
        /// Delete Department.
        /// </summary>

        [HttpDelete("DeleteDepartment")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "SAdmin,Admin")]
        [EnableRateLimiting("AdminSoftDeletePolicy")]
        public async Task<IActionResult> DeleteDepartment(int departmentId)
        {
            if (departmentId <= 0)
            {
                return BadRequest("Invalid department ID.");
            }
            var ISDelete = await _mediator.Send(new DeleteDepartmentCommand(departmentId));
            if (!ISDelete)
            {
                return NotFound("Department not found.");
            }

            return Ok("Department deleted successfully.");
        }

        /// <summary>
        /// Get Departments.
        /// </summary>
        [HttpGet("GetDepartments")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "SAdmin,Admin")]
        [EnableRateLimiting("AdminGetAllPolicy")]
        public async Task<IActionResult> GetDepartments([FromQuery] string? departmentName)
        {
            var departments = await _mediator.Send(new GetAllDepartment(departmentName));
            if (departments == null || !departments.Any())
            {
                return NotFound("No departments found.");
            }

            return Ok(departments);


        }
    }
}
