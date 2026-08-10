using ATLAsBLL.Services.CourseOffering.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ATLASDomain.DTO.CourseOffring;
using Microsoft.AspNetCore.Authorization;

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
        [Authorize(Roles = "Student")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<CourseOfferingDTO>>> GetStudentMyCourses(int studentId)
        {
            if (!int.TryParse(User.FindFirst("StudentId")?.Value, out var authenticatedStudentId))
                return Unauthorized();

            if (studentId != authenticatedStudentId)
                return Forbid();
            if (studentId < 1)
            {
                return BadRequest("Invalid student ID.");
            }
            var courses = await _mediator.Send(new ATLAsBLL.Services.CourseOffering.Queries.GetmyCourse.Student.GetMyCourseQuery(studentId));
            if (courses == null || !courses.Any())
            {
                return NotFound($"No courses found for student ID {studentId}.");
            }
            return Ok(courses);
        }



        [HttpGet("Staff/MyCourses", Name = "GetStaffMyCourses")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<DoctorCourseOfferingDTO>>> GetStaffMyCourses(int StaffId)
        {
            if (!int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var authenticatedUserId))
                return Unauthorized();

            if (StaffId != authenticatedUserId)
                return Forbid();
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
        [Authorize(Roles = "Student")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<CourseofferingDetailsDTO>>> GetCourseDetails(int courseOfferingId, int studentId)
        {
            if (!int.TryParse(User.FindFirst("StudentId")?.Value, out var authenticatedStudentId))
                return Unauthorized();

            if (studentId != authenticatedStudentId)
                return Forbid();
            if (studentId < 1 || courseOfferingId < 1)
            {
                return BadRequest("Invalid student ID or course offering ID.");
            }
            var courses = await _mediator.Send(new ATLAsBLL.Services.CourseOffering.Queries.GetCourseDetails.GetCourseDetailsQuery(courseOfferingId, studentId));
            if (courses == null || !courses.Any())
            {
                return NotFound($"No course details found for CourseOfferingId {courseOfferingId} and StudentId {studentId}.");
            }
            return Ok(courses);
        }



    }
}
