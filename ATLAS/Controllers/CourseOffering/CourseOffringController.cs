using ATLAsBLL.Services.CourseOffering.Queries;
using ATLASDomain.DTO.CourseOffring;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ATLAS.Controllers.CourseOffering

{
    [Route("api/CourseOffring")]
    [ApiController]
    public class CourseOffringController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CourseOffringController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpGet("Student/MyCourses", Name = "GetStudentMyCourses")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Student")]
        [EnableRateLimiting("CourseQueryPolicy")]
        public async Task<ActionResult<IEnumerable<CourseOfferingDTO>>> GetStudentMyCourses()
        {
            var userId = GetCurrentUserId();

           
            if (userId < 1)
            {
                return BadRequest("Invalid Request.");
            }
            var courses = await _mediator.Send(new ATLAsBLL.Services.CourseOffering.Queries.GetmyCourse.Student.GetMyCourseQuery(userId));
            if (courses == null || !courses.Any())
            {
                return NotFound($"No courses found for student ID {userId}.");
            }
            return Ok(courses);
        }



        [HttpGet("Staff/MyCourses", Name = "GetStaffMyCourses")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Doctor,TeachingAssistant")]
        [EnableRateLimiting("CourseQueryPolicy")]
        public async Task<ActionResult<IEnumerable<DoctorCourseOfferingDTO>>> GetStaffMyCourses()
        {
            var StaffId = GetCurrentUserId();
            if (StaffId < 1)
            {
                return BadRequest("Invalid Staff ID.");
            }
            var courses = await _mediator.Send(new ATLAsBLL.Services.CourseOffering.Queries.GetmyCourse.Doctor.GetStaffMyCourseQuery(StaffId));
            if (courses == null || !courses.Any())
            {
                return NotFound($"No courses found for Staff ID {StaffId}.");
            }
            return Ok(courses);
        }


        [HttpGet("CourseDetails", Name = "GetCourseDetails")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Student")]
        [EnableRateLimiting("CourseDetailsPolicy")]
        public async Task<ActionResult<IEnumerable<CourseofferingDetailsDTO>>> GetCourseDetails(int courseOfferingId)
        {
            var userId = GetCurrentUserId();
            if (userId < 1 || courseOfferingId < 1)
            {
                return BadRequest("Invalid student ID or course offering ID.");
            }
            var courses = await _mediator.Send(new ATLAsBLL.Services.CourseOffering.Queries.GetCourseDetails.GetCourseDetailsQuery(courseOfferingId, userId));
            if (courses == null || !courses.Any())
            {
                return NotFound($"No course details found for CourseOfferingId {courseOfferingId} and StudentId {userId}.");
            }
            return Ok(courses);
        }

        [HttpGet("DoctorIDs", Name = "GetDoctorIDs")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Student")]
        [EnableRateLimiting("CourseQueryPolicy")]
        public async Task<ActionResult<IEnumerable<int>>> GetDoctorIDs(int courseOfferingId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId < 1 || courseOfferingId < 1)
                {
                    return BadRequest("Invalid user ID or course offering ID.");
                }
                var doctorIDs = await _mediator.Send(new ATLAsBLL.Services.CourseOfferingDoctor.Query.GetDoctorIDQuery(courseOfferingId));
                if (doctorIDs == null || !doctorIDs.Any())
                {
                    return NotFound($"No doctors found for CourseOfferingId {courseOfferingId}.");
                }
                return Ok(doctorIDs);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception)
            {
                return Problem(statusCode: StatusCodes.Status500InternalServerError, title: "An unexpected error occurred.");
            }
        }
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token.");
            }
            return userId;
        }


    }
}
