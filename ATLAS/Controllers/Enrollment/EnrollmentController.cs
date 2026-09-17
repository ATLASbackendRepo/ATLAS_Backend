using ATLAsBLL.Features.Enrollments.Commands.CreateEnrollment;
using ATLAsBLL.Features.Enrollments.Commands.CreateEnrollmentsRange;
using ATLAsBLL.Features.Enrollments.Commands.DeleteEnrollment;
using ATLAsBLL.Services.Enrollment.Query;
using ATLASDomain.DTO.Enrollment;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ATLAS.Controllers.Enrollment
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SAdmin,Admin")]
    public class EnrollmentController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EnrollmentController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

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

            var result = await _mediator.Send(new CreateEnrollmentCommand(academicId, dto));
            if (!result)
            {
                return BadRequest("Failed to create enrollment.");
            }

            return Ok("Enrollment created successfully.");
        }

        [HttpPost("range")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [EnableRateLimiting("AdminCreateRangePolicy")]
        public async Task<IActionResult> CreateRangeEnrollment([FromBody] BulkCreateEnrollmentDTO dto)
        {
            try
            {
                var result = await _mediator.Send(new CreateEnrollmentsRangeCommand(dto));
                if (!result)
                {
                    return BadRequest("Failed to create enrollment range.");
                }

                return Ok("Enrollments range created successfully.");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

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