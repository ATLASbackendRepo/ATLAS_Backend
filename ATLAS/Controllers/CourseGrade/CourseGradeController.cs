//using ATLAsBLL.Features.CourseGrades.Commands.CreateCourseGradesRange;
using ATLAS;
using ATLAsBLL.Features.CourseGrades.Commands.CreateCourseGradesRange;
using ATLAsBLL.Features.CourseGrades.Commands.UpdateCourseGrade;
using ATLAsBLL.Features.CourseGrades.Queries.ExportCourseGrades;
using ATLAsBLL.Services.CourseGrade.Queries;
using ATLASDomain.DTO.CourseGrade;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.Tasks;
namespace ATLASAPI.Controllers
{
    /// <summary>
    /// Course Grade Controller.
    /// </summary>
    [ApiController]
    [Route("api/CourseGrade")]
    public class CourseGradeController : ControllerBase
    {
        /// <summary>
        /// _mediator.
        /// </summary>
        private readonly IMediator _mediator;
        /// <summary>
        /// _course Authorization.
        /// </summary>
        private readonly ICourseOfferingAuthorizationService _courseAuthorization;

        public CourseGradeController(IMediator mediator, ICourseOfferingAuthorizationService courseAuthorization)
        {
            _mediator = mediator;
            _courseAuthorization = courseAuthorization;
        }

        /// <summary>
        /// Retrieves a paginated list of student grade cards for a specific course offering with search capability.
        /// </summary>
        /// <param name="courseOfferingId">The ID of the course offering.</param>
        /// <param name="pageNumber">Page number to fetch (default is 1).</param>
        /// <param name="pageSize">Number of items per page (default is 9).</param>
        /// <param name="searchTerm">Optional search term to filter students by name, academic ID, or code.</param>
      
        
        
        /// <summary>
        /// Get Course Students Paged.
        /// </summary>
        [HttpGet("course-offering/{courseOfferingId:int}/students")]
        [ProducesResponseType(typeof(PagedResultDto<StudentCardDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Doctor,TeachingAssistant,Admin,SAdmin")]
        [EnableRateLimiting("CourseGradeReadPolicy")]
        public async Task<ActionResult<PagedResultDto<StudentCardDto>>> GetCourseStudentsPaged(
            int courseOfferingId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 9,
            [FromQuery] string? searchTerm = null)
        {
            // Input validation for route and pagination parameters
            if (courseOfferingId <= 0 || pageNumber <= 0 || pageSize <= 0 || pageSize > GetStudentsPagedQueryModel.MaxPageSize)
            {
                return BadRequest(new { Message = $"Invalid parameters. Page size must be between 1 and {GetStudentsPagedQueryModel.MaxPageSize}." });
            }

            if (!await _courseAuthorization.CanAccessOfferingAsync(User, courseOfferingId, HttpContext.RequestAborted))
                return Forbid();

            var queryModel = new GetStudentsPagedQueryModel
            {
                CourseOfferingId = courseOfferingId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm
            };

            var result = await _mediator.Send(new GetCourseStudentsQuery(queryModel));

            return Ok(result);
        }



        /// <summary>
        /// Assign Grade.
        /// </summary>
        [HttpPost("assign")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Doctor,TeachingAssistant,Admin,SAdmin")]
        [EnableRateLimiting("CourseGradeAssignPolicy")]
        public async Task<IActionResult> AssignGrade(
             [FromBody] AssignCourseGradeCommand command)
        {
            if (command == null || command.EnrollmentId <= 0 || command.ModuleId <= 0)
            {
                return BadRequest("Invalid grade data.");
            }

            if (!int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var userId))
                return Unauthorized();

            if (!await _courseAuthorization.CanManageGradeAsync(User, command.EnrollmentId, command.ModuleId, HttpContext.RequestAborted))
                return Forbid();

            command = command with { UserId = userId };

            var result = await _mediator.Send(command);

            if (!result)
                return NotFound("Failed to assign grade.");

            return Ok(new
            {
                Success = true,
                Message = "Grade assigned successfully."
            });
        }



        //[HttpPost("assign-range")]
        //[Authorize(Roles = "Doctor,TeachingAssistant,Admin,SAdmin")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //public async Task<IActionResult> AssignGrades(
        //   [FromBody] InsertGradeDTO gradeDTO)
        //{
        //    if (gradeDTO == null || gradeDTO.EnrollmentId <= 0 || gradeDTO.ModuleId <= 0)
        //    {
        //        return BadRequest("Invalid grade data.");
        //    }

        //    if (!int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var userId))
        //        return Unauthorized();

        //    if (!await _courseAuthorization.CanManageGradeAsync(User, gradeDTO.EnrollmentId, gradeDTO.ModuleId, HttpContext.RequestAborted))
        //        return Forbid();

        //    gradeDTO.UserId = userId;


        //    var result = await _mediator.Send(new AssignCourseGradesRangeCommand(new List<InsertGradeDTO> { gradeDTO }));

        //    if (result == false)
        //        return NotFound("Failed to assign grades.");

        //    return Ok(new
        //    {
        //        Success = true,
        //        Message = "Grades assigned successfully."
        //    });
        //}

        /// <summary>
        /// Assign Grades Range.
        /// </summary>
        [HttpPost("assign-range")]
        [Authorize(Roles = "Doctor,TeachingAssistant,Admin,SAdmin")]
        [EnableRateLimiting("CourseGradeAssignPolicy")]
        public async Task<ActionResult> AssignGradesRange(
            IFormFile file,
            int courseOfferingId,
            int modelId,
            CancellationToken cancellationToken)
                {
            try
            {
                const long maxUploadBytes = 10 * 1024 * 1024;

                var extension = Path
                    .GetExtension(file?.FileName ?? string.Empty)
                    .ToLowerInvariant();

                if (file == null ||
                    file.Length == 0 ||
                    file.Length > maxUploadBytes ||
                    extension is not ".xlsx" and not ".xls")
                {
                    return BadRequest(
                        "Upload a non-empty Excel file no larger than 10 MB.");
                }

                // Validate Excel file signature
                var signature = new byte[8];

                await using (var stream = file.OpenReadStream())
                {
                    var bytesRead = await stream.ReadAsync(
                        signature,
                        cancellationToken);

                    var isXlsx = bytesRead >= 2 &&
                                 signature.Take(2)
                                     .SequenceEqual(
                                         new byte[] { 0x50, 0x4B });

                    var isXls = bytesRead >= 8 &&
                                signature
                                    .Take(8)
                                    .SequenceEqual(
                                        new byte[]
                                        {
                                    0xD0, 0xCF, 0x11, 0xE0,
                                    0xA1, 0xB1, 0x1A, 0xE1
                                        });

                    if ((extension == ".xlsx" && !isXlsx) ||
                        (extension == ".xls" && !isXls))
                    {
                        return BadRequest(
                            "The uploaded file does not match its Excel format.");
                    }
                }

                // Get current user ID
                if (!await _courseAuthorization.CanAccessOfferingAsync(User, courseOfferingId, cancellationToken))
                    return Forbid();

                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

                if (userIdClaim == null ||
                    !int.TryParse(userIdClaim.Value, out var userId))
                {
                    return Unauthorized("Invalid user identity.");
                }

                // Create temporary upload folder
                var uploadsFolder = Path.Combine(
                    Path.GetTempPath(),
                    "ATLAS",
                    "uploads");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filename = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(
                    uploadsFolder,
                    filename);

                // Save uploaded file
                await using (var stream = new FileStream(
                    filePath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None))
                {
                    await file.CopyToAsync(
                        stream,
                        cancellationToken);
                }

                var command = new AssignCourseGradesRangeCommand(
                    filePath,
                    courseOfferingId,
                    userId,
                    modelId);

                var result = await _mediator.Send(
                    command,
                    cancellationToken);

                return Ok(new
                {
                    success = result,
                    message = "Course grades assigned successfully."
                });
            }
            catch (OperationCanceledException)
            {
                return BadRequest("The upload was canceled.");
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
            catch (Exception)
            {
                return BadRequest("Invalid data.");
            }
        }








        /// <summary>
        /// Export Grades.
        /// </summary>
        [HttpGet("export")]
        [Authorize(Roles = "Doctor,TeachingAssistant,Admin,SAdmin")]
        [EnableRateLimiting("CourseGradeAssignPolicy")]
        public async Task<IActionResult> ExportGrades(
         [FromQuery] int courseOfferingId,
         [FromQuery] string format = "excel",
         CancellationToken cancellationToken = default)
        {
            try
            {
                if (courseOfferingId <= 0)
                    return BadRequest(new { message = "Course offering ID must be greater than zero." });

                if (!await _courseAuthorization.CanAccessOfferingAsync(User, courseOfferingId, cancellationToken))
                    return Forbid();

                var result = await _mediator.Send(
                    new ExportCourseGradesQuery(
                        courseOfferingId,
                        format),
                    cancellationToken);

                return File(
                    result.File,
                    result.ContentType,
                    result.FileName);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }




    }


}
