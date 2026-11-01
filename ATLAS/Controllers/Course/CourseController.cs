using ATLAsBLL.Features.Courses.Commands.CreateCourse;
using ATLAsBLL.Features.Courses.Commands.DeleteCourse;
using ATLAsBLL.Features.Courses.Commands.UpdateCourse;
using ATLAsBLL.Services.Course.Queries;
using ATLASDomain.DTO.Course;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAS.Controllers.Course
{
    /// <summary>
    /// Course Controller.
    /// </summary>
    [Route("api/Course")]
    [ApiController]
    public class CourseController : Controller
    {
        private readonly IMediator _mediator;

        public CourseController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create Course.
        /// </summary>
        [HttpPost("CreateCourse")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [Authorize(Roles = "SAdmin,Admin")]
        [EnableRateLimiting("AdminUpdatePolicy")]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDTO createCourseDTO, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(createCourseDTO.CourseName) || createCourseDTO.CreditHours <= 0)
            {
                return BadRequest("Invalid course data.");
            }

            var courseId = await _mediator.Send(new CreateCourseCommand(createCourseDTO), cancellationToken);
            if (courseId <= 0)
            {
                return BadRequest("Failed to create Course.");
            }

            return StatusCode(StatusCodes.Status201Created, $"Course created successfully. Id: {courseId}");
        }

        /// <summary>
        /// Update Course.
        /// </summary>
        [HttpPut("UpdateCourse")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [Authorize(Roles = "SAdmin,Admin")]
        [EnableRateLimiting("AdminUpdatePolicy")]
        public async Task<IActionResult> UpdateCourse([FromBody] UpdateCourseDTO updateCourse, CancellationToken cancellationToken = default)
        {
            if (updateCourse.Id <= 0 || string.IsNullOrWhiteSpace(updateCourse.CourseName))
            {
                return BadRequest("Invalid Course data.");
            }

            var result = await _mediator.Send(new UpdateCourseCommand(updateCourse), cancellationToken);
            if (result)
            {
                return Ok("Course updated successfully.");
            }

            return BadRequest("Failed to update Course.");
        }

        /// <summary>
        /// Delete Course.
        /// </summary>
        [HttpDelete("DeleteCourse")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [Authorize(Roles = "SAdmin,Admin")]
        [EnableRateLimiting("AdminSoftDeletePolicy")]
        public async Task<IActionResult> DeleteCourse(int CourseId, CancellationToken cancellationToken = default)
        {
            if (CourseId <= 0)
            {
                return BadRequest("Invalid Course ID.");
            }

            var isDeleted = await _mediator.Send(new DeleteCourseCommand(CourseId), cancellationToken);
            if (!isDeleted)
            {
                return NotFound("Course not found.");
            }

            return Ok("Course deleted successfully.");
        }

        /// <summary>
        /// Get Courses.
        /// </summary>
        [HttpGet("GetCourses")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize(Roles = "SAdmin,Admin")]
        [EnableRateLimiting("AdminGetAllPolicy")]
        public async Task<IActionResult> GetCourses([FromQuery] string? Coursename, CancellationToken cancellationToken = default)
        {
            var courses = await _mediator.Send(new GetAllCourseQuery(Coursename), cancellationToken);
            return Ok(courses ?? new List<CourseDTO>());
        }
    }
}
