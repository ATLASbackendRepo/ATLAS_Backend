using ATLAsBLL.Features.CourseGrades.Commands.CreateCourseGradesRange;
using ATLAsBLL.Features.CourseGrades.Commands.UpdateCourseGrade;
using ATLAsBLL.Services.CourseGrade.Queries.StudentGradeView;
using ATLASDomain.DTO.CourseGrade;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ATLAS;
namespace ATLASAPI.Controllers
{
    [ApiController]
    [Route("api/CourseGrade")]
    public class CourseGradeController : ControllerBase
    {
        private readonly IMediator _mediator;
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
        [HttpGet("course-offering/{courseOfferingId:int}/students")]
        [Authorize(Roles = "Doctor,TeachingAssistant,Admin,SAdmin")]
        [ProducesResponseType(typeof(PagedResultDto<StudentCardDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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



        [HttpPost("assign")]
        [Authorize(Roles = "Doctor,TeachingAssistant,Admin,SAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignGrade(
             [FromBody] AssignCourseGradeCommand command)
        {
            if(command == null || command.EnrollmentId <= 0 || command.ModuleId <= 0)
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



        [HttpPost("assign-range")]
        [Authorize(Roles = "Doctor,TeachingAssistant,Admin,SAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignGrades(
           [FromBody] InsertGradeDTO gradeDTO)
        {
            if (gradeDTO == null || gradeDTO.EnrollmentId <= 0 || gradeDTO.ModuleId <= 0)
            {
                return BadRequest("Invalid grade data.");
            }

            if (!int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var userId))
                return Unauthorized();

            if (!await _courseAuthorization.CanManageGradeAsync(User, gradeDTO.EnrollmentId, gradeDTO.ModuleId, HttpContext.RequestAborted))
                return Forbid();

            gradeDTO.UserId = userId;


            var result = await _mediator.Send(new AssignCourseGradesRangeCommand(new List<InsertGradeDTO> { gradeDTO }));

            if (result == false)
                return NotFound("Failed to assign grades.");

            return Ok(new
            {
                Success = true,
                Message = "Grades assigned successfully."
            });
        }



    }
}
