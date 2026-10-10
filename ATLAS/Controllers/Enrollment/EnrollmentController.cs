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
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [EnableRateLimiting("AdminUpdatePolicy")]
       
        public async Task<IActionResult> CreateEnrollment([FromQuery] string academicId, [FromBody] BulkCreateEnrollmentDTO dto, CancellationToken cancellationToken = default)
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

            
                var result = await _mediator.Send(
                    new CreateEnrollmentCommand(academicId, dto, userId), cancellationToken);

                if (!result)
                {
                    return BadRequest(
                        "The student is already enrolled in this course offering.");
                }

                // LOW-07: creating an enrollment is a 201, not a 200.
                return StatusCode(StatusCodes.Status201Created, "Enrollment created successfully.");
            
        }

        /// <summary>
        /// Creates bulk student enrollments from an uploaded Excel file (.xlsx).
        /// </summary>
        /// <param name="file">The uploaded Excel file (.xlsx) containing 'AcademicID' and 'Name' columns.</param>
        /// <param name="dto">The course offering details (CourseID, AcademicYear, SemesterName, DepartmentID, level).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A detailed summary of rows processed, enrollments created, skipped duplicates, and missing Academic IDs.</returns>
        [HttpPost("range")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(BulkEnrollmentResultDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [EnableRateLimiting("AdminCreateRangePolicy")]
        public async Task<IActionResult> CreateRangeEnrollment(
            IFormFile file,
            [FromForm] BulkCreateEnrollmentDTO dto,
            CancellationToken cancellationToken = default)
        {
            const long maxUploadBytes = 10 * 1024 * 1024; // 10 MB limit

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim is null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized();
            }

            if (dto is null)
            {
                return BadRequest(" enrollments details are required.");
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest("Upload a non-empty Excel file.");
            }

            if (file.Length > maxUploadBytes)
            {
                return BadRequest("File size exceeds maximum allowed limit of 10 MB.");
            }

            var extension = Path.GetExtension(file.FileName ?? string.Empty).ToLowerInvariant();
            if (extension is not ".xlsx" and not ".xls")
            {
                return BadRequest("Only Excel files (.xlsx or .xls) are supported.");
            }

            // Validate magic bytes signature for Excel files
            var signature = new byte[8];
            await using (var stream = file.OpenReadStream())
            {
                var bytesRead = await stream.ReadAsync(signature, 0, signature.Length, cancellationToken);
                var isXlsx = signature.Take(2).SequenceEqual(new byte[] { 0x50, 0x4B });
                var isXls = signature.Take(bytesRead).SequenceEqual(new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 });
                if ((extension == ".xlsx" && !isXlsx) || (extension == ".xls" && !isXls))
                {
                    return BadRequest("The uploaded file content does not match its Excel file extension.");
                }
            }

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;

          
                var result = await _mediator.Send(
                    new CreateEnrollmentsRangeCommand(memoryStream, file.FileName, dto, userId),
                    cancellationToken);

                return StatusCode(StatusCodes.Status201Created, result);
           
        }
       /// <summary>
        /// Delete Enrollment.
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [EnableRateLimiting("AdminSoftDeletePolicy")]
 
        public async Task<IActionResult> DeleteEnrollment(int id, CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid enrollment ID.");
            }

            var isDeleted = await _mediator.Send(new DeleteEnrollmentCommand(id), cancellationToken);
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
            [FromQuery] int pageIndex = 1, CancellationToken cancellationToken = default)
        {
            var enrollments = await _mediator.Send(new GetEnrollment(searchKeyword, doctorId, pageIndex), cancellationToken);
            if (enrollments == null)
            {
                return NotFound("No enrollments found.");
            }

            return Ok(enrollments);
        }
    }
}
