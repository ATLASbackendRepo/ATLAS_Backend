using ATLAsBLL.Features.AssessmentModules.Commands.CreateAssessmentModule;
using ATLAsBLL.Features.AssessmentModules.Commands.DeleteAssessmentModule;
using ATLAsBLL.Features.AssessmentModules.Commands.UpdateAssessmentModule;
using ATLAsBLL.Services.AssessmentModule.Qureies;
using ATLASDomain.DTO.AssessmentModule;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace ATLAS.Controllers.AssessmentModule
{

    /// <summary>
    /// Module Controller.
    /// </summary>
    [Authorize]
    [Route("api/Module")]
    public class ModuleController : Controller
    {
        /// <summary>
        /// _mediator.
        /// </summary>
        private readonly IMediator _mediator;
        /// <summary>
        /// _course Authorization.
        /// </summary>
        private readonly ICourseOfferingAuthorizationService _courseAuthorization;

        public ModuleController(IMediator mediator, ICourseOfferingAuthorizationService courseAuthorization)
        {
            _mediator = mediator;
            _courseAuthorization = courseAuthorization;
        }

        /// <summary>
        /// Get Current User Id.
        /// </summary>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token.");
            }
            return userId;
        }

        /// <summary>
        /// Display Assessment Module.
        /// </summary>
        [HttpGet("DisplayAssessmentModule")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Doctor,TeachingAssistant")]
        [EnableRateLimiting("CourseGradeReadPolicy")]
        public async Task<ActionResult<IEnumerable<AssessmentModelDisplayDTO>>> DisplayAssessmentModule([FromQuery] int courseOfferingId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!await _courseAuthorization.CanAccessOfferingAsync(User, courseOfferingId, cancellationToken))
                    return Forbid();
                var command = new AssessmentModelDisplayQuery(courseOfferingId);
                var modules = await _mediator.Send(command, cancellationToken);
                return Ok(modules);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Authentication is required to perform this operation." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "The requested resource was not found." });
            }
            catch (ArgumentException)
            {
                return BadRequest(new { message = "The request is invalid." });
            }
            catch (Exception)
            {
                return Problem(statusCode: StatusCodes.Status500InternalServerError, title: "An unexpected error occurred.");
            }
        }

        /// <summary>
        /// Create Assessment Module.
        /// </summary>
        [HttpPost("CreateAssessmentModule")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Doctor,TeachingAssistant")]
        [EnableRateLimiting("CourseGradeReadPolicy")]
        public async Task<ActionResult<int>> CreateAssessmentModule([FromBody] CreateModeDTO createModeDTO, CancellationToken cancellationToken = default)
        { 
            try
            {
                var userId = GetCurrentUserId();
                var command = new CreateAssessmentModuleCommand(createModeDTO , userId);
                var moduleId = await _mediator.Send(command, cancellationToken);
                return Ok(moduleId);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Authentication is required to perform this operation." });
            }
            catch (ValidationException validationException)
            {
                // A rejected request (for example MaxDegree outside the allowed
                // range) is a client mistake, not a server fault (MED-07).
                return BadRequest(new
                {
                    message = "The request is invalid.",
                    errors = validationException.Errors
                        .GroupBy(failure => failure.PropertyName)
                        .ToDictionary(
                            group => group.Key,
                            group => group.Select(failure => failure.ErrorMessage).ToArray())
                });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new { message = exception.Message });
            }
            catch (Exception)
            {
                return Problem(statusCode: StatusCodes.Status500InternalServerError, title: "An unexpected error occurred.");
            }
        }


        /// <summary>
        /// Update Assessment Module.
        /// </summary>
        [HttpPut("UpdateAssessmentModule")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Doctor,TeachingAssistant")]
        [EnableRateLimiting("CourseGradeReadPolicy")]
        public async Task<ActionResult<int>> UpdateAssessmentModule([FromBody] UpdateModeDTO updateModeDTO, CancellationToken cancellationToken = default)
        { 
            try
            {
                var command = new UpdateAssessmentModuleCommand(updateModeDTO, GetCurrentUserId());
                var moduleId = await _mediator.Send(command, cancellationToken);
                return Ok(moduleId);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Authentication is required to perform this operation." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "The requested resource was not found." });
            }
            catch (ValidationException validationException)
            {
                // A rejected request (for example MaxDegree outside the allowed
                // range) is a client mistake, not a server fault (MED-07).
                return BadRequest(new
                {
                    message = "The request is invalid.",
                    errors = validationException.Errors
                        .GroupBy(failure => failure.PropertyName)
                        .ToDictionary(
                            group => group.Key,
                            group => group.Select(failure => failure.ErrorMessage).ToArray())
                });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new { message = exception.Message });
            }
            catch (Exception)
            {
                return Problem(statusCode: StatusCodes.Status500InternalServerError, title: "An unexpected error occurred.");
            }
        }
        /// <summary>
        /// Delete Assessment Module.
        /// </summary>
        [HttpDelete("DeleteAssessmentModule")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Doctor,TeachingAssistant")]
        [EnableRateLimiting("CourseGradeReadPolicy")]
        public async Task<ActionResult<int>> DeleteAssessmentModule([FromQuery] int moduleId, CancellationToken cancellationToken = default)
        { 
            try
            {
                var command = new DeleteAssessmentModuleCommand(moduleId, GetCurrentUserId());
                var result = await _mediator.Send(command, cancellationToken);
                return Ok(moduleId);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Authentication is required to perform this operation." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "The requested resource was not found." });
            }
            catch (ArgumentException)
            {
                return BadRequest(new { message = "The request is invalid." });
            }
            catch (Exception)
            {
                return Problem(statusCode: StatusCodes.Status500InternalServerError, title: "An unexpected error occurred.");
            }
        }



    }
}
