using ATLAsBLL.Features.Courses.Commands.CreateCourse;
using ATLAsBLL.Features.Courses.Commands.DeleteCourse;
using ATLAsBLL.Features.Courses.Commands.UpdateCourse;
using ATLAsBLL.Services.Course.Queries;
using ATLASDomain.DTO.Course;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "SAdmin,Admin")]
        [EnableRateLimiting("AdminUpdatePolicy")]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDTO createCourseDTO)
        {
            try
            {
                if (!string.IsNullOrEmpty(createCourseDTO.CourseName) && createCourseDTO.CreditHours > 0)
                {
                    var CourseID = await _mediator.Send(new CreateCourseCommand(createCourseDTO));
                    if (CourseID <= 0)
                    {
                        return BadRequest("Failed to create Course.");
                    }
                    return Ok("Course created successfully.");
                }
                return BadRequest();
            }
            catch
            {
                return BadRequest("Failed to create Course.");
            }
        }

        /// <summary>
        /// Update Course.
        /// </summary>
        [HttpPut("UpdateCourse")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "SAdmin,Admin")]
        [EnableRateLimiting("AdminUpdatePolicy")]
        public async Task<IActionResult> UpdateCourse([FromBody] UpdateCourseDTO updateCourse)
        {
            try
            {
                if (updateCourse.Id > 0 && !string.IsNullOrEmpty(updateCourse.CourseName))
                {
                    var result = await _mediator.Send(new UpdateCourseCommand(updateCourse));
                    if (result)
                        return Ok("Department updated successfully.");

                }

                return BadRequest("Fialed To Updated Course");
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Department not found.");
            }
            catch
            {
                return BadRequest("Fialed To Updated Course");
            }
        }

            /// <summary>
            /// Delete Course.
            /// </summary>
            [HttpDelete("DeleteCourse")]
            [ProducesResponseType(StatusCodes.Status400BadRequest)]
            [ProducesResponseType(StatusCodes.Status404NotFound)]
            [ProducesResponseType(StatusCodes.Status200OK)]
            [Authorize(Roles = "SAdmin,Admin")]
            [EnableRateLimiting("AdminSoftDeletePolicy")]
            public async Task<IActionResult> DeleteCourse(int CourseId)
            {
                if (CourseId <= 0)
                {
                    return BadRequest("Invalid Course ID.");
                }
                var ISDelete = await _mediator.Send(new DeleteCourseCommand(CourseId));
                if (!ISDelete)
                {
                    return NotFound("Department not found.");
                }

                return Ok("Department deleted successfully.");
            }


        /// <summary>
        /// Get Courses.
        /// </summary>
        [HttpGet("GetCourses")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "SAdmin,Admin")]
        [EnableRateLimiting("AdminGetAllPolicy")]
        public async Task<IActionResult> GetCourses([FromQuery] string? Coursename)
        {
            var departments = await _mediator.Send(new GetAllCourseQuery(Coursename));
            if (departments == null || !departments.Any())
            {
                return NotFound("No departments found.");
            }

            return Ok(departments);


        }
    }
}
