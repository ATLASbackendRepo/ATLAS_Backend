using ATLAsBLL.Features.Enrollments.Commands.CreateEnrollment;
using ATLAsBLL.Features.Enrollments.Commands.CreateEnrollmentsRange;
using ATLAsBLL.Features.Enrollments.Commands.DeleteEnrollment;
using ATLAsBLL.Services.Enrollment.Query;
using ATLASDomain.DTO.Enrollment;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace ATLAS.Controllers.Enrollment
{    /// <summary>
    /// Enrollment Controller.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SAdmin,Admin")]

    public class EnrollmentController : ControllerBase
    {
        /// <summary>
        /// _mediator.
        /// </summary>
        private readonly IMediator _mediator;

        public EnrollmentController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }
         /// <summary>
        /// Create Enrollment.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [EnableRateLimiting("AdminUpdatePolicy")]
       
        public async Task<IActionResult> CreateEnrollment([FromQuery] string academicId, [FromBody] BulkCreateEnrollmentDTO dto)
        {
            if (string.IsNullOrWhiteSpace(academicId))
            {
                return BadRequest("Academic ID is required.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim is null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var result = await _mediator.Send(
                    new CreateEnrollmentCommand(academicId, dto, userId));

                if (!result)
                {
                    return BadRequest(
                        "The student is already enrolled in this course offering.");
                }

                return Ok("Enrollment created successfully.");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Create Range Enrollment.
        /// </summary>
        [HttpPost("range")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [EnableRateLimiting("AdminCreateRangePolicy")]
        
        public async Task<IActionResult> CreateRangeEnrollment([FromBody] BulkCreateEnrollmentDTO dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim is null || !int.TryParse(userIdClaim.Value, out var userId))
                {
                    return Unauthorized();
                }

                var result = await _mediator.Send(
                    new CreateEnrollmentsRangeCommand(dto, userId));

                if (!result)
                {
                    return BadRequest(
                        "No student matched the enrollment filter, or they are already enrolled.");
                }

                return Ok("Enrollments range created successfully.");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
       /// <summary>
        /// Delete Enrollment.
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [EnableRateLimiting("AdminSoftDeletePolicy")]
 
        public async Task<IActionResult> DeleteEnrollment(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid enrollment ID.");
            }

            var isDeleted = await _mediator.Send(new DeleteEnrollmentCommand(id));
            if (!isDeleted)
            {
                return NotFound($"Enrollment with ID {id} not found.");
            }

            return Ok("Enrollment deleted successfully.");
        }
        /// <summary>
        /// Get Enrollments.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [EnableRateLimiting("AdminGetAllPolicy")]

        public async Task<IActionResult> GetEnrollments(
            [FromQuery] string? searchKeyword = null,
            [FromQuery] int? doctorId = null,
            [FromQuery] int pageIndex = 1)
        {
            var enrollments = await _mediator.Send(new GetEnrollment(searchKeyword, doctorId, pageIndex));
            if (enrollments == null)
            {
                return NotFound("No enrollments found.");
            }

            return Ok(enrollments);
        }
    }
}
