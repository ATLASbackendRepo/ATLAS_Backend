using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using ATLAsBLL.Services.CourseOffering.Queries.Analysis;
using ATLASDomain.DTO.Analysis;
using ATLAS;

namespace ATLASAPI.Controllers
{
    [ApiController]
    [Route("api/Analysis")]
    public class AnalysisController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICourseOfferingAuthorizationService _courseAuthorization;

        public AnalysisController(IMediator mediator, ICourseOfferingAuthorizationService courseAuthorization)
        {
            _mediator = mediator;
            _courseAuthorization = courseAuthorization;
        }


        [HttpGet("course-offering/{courseOfferingId:int}")]
        [Authorize(Roles = "Doctor,TeachingAssistant,Admin,SAdmin")]
        [ProducesResponseType(typeof(CourseAnalysisResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CourseAnalysisResponseDto>> GetCourseAnalysis(int courseOfferingId)
        {
         
            if (courseOfferingId <= 0)
            {
                return BadRequest(new { Message = "Invalid course offering ID. ID must be greater than 0." });
            }

            if (!await _courseAuthorization.CanAccessOfferingAsync(User, courseOfferingId, HttpContext.RequestAborted))
                return Forbid();

            var result = await _mediator.Send(new CourseAnalysisquery(courseOfferingId));

            
            if (result == null || (!result.ModuleAverages.Any() && result.Distribution.ClassAvg == 0))
            {
                return NotFound(new { Message = $"No analysis data found for course offering ID: {courseOfferingId}." });
            }

            return Ok(result);
        }
    }
}
