using ATLAsBLL.Services.CourseOffering.Queries;
using ATLAsBLL.Services.User.qeusies.GetUserID;
using ATLASDomain.DTO.CourseOffring;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ATLAS.Controllers.CourseOffering

{
    /// <summary>
    /// Course Offring Controller.
    /// </summary>
    [Route("api/CourseOffring")]
    [ApiController]
    public class CourseOffringController : ControllerBase
    {
        /// <summary>
        /// _mediator.
        /// </summary>
        private readonly IMediator _mediator;
        /// <summary>
        /// _logger.
        /// </summary>
        private readonly ILogger<CourseOffringController> _logger;

        public CourseOffringController(IMediator mediator, ILogger<CourseOffringController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }


        /// <summary>
        /// Get Student My Courses.
        /// </summary>
        [HttpGet("Student/MyCourses", Name = "GetStudentMyCourses")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Student")]
        [EnableRateLimiting("CourseQueryPolicy")]
        public async Task<ActionResult<IEnumerable<CourseOfferingDTO>>> GetStudentMyCourses(CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();
            if (userId < 1)
            {
                return Unauthorized();
            }

            var courses = await _mediator.Send(new ATLAsBLL.Services.CourseOffering.Queries.GetmyCourse.Student.GetMyCourseQuery(userId), cancellationToken);
            if (courses == null || !courses.Any())
            {
                return NotFound($"No courses found for student ID {userId}.");
            }
            return Ok(courses);
        }




        /// <summary>
        /// Get Admin Student My Courses.
        /// </summary>
        [HttpGet("Admin/Student/MyCourses/{AcdimicID}", Name = "GetAdminStudentMyCourses")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "SAdmin,Admin")]
        [EnableRateLimiting("CourseQueryPolicy")]
        public async Task<ActionResult<IEnumerable<CourseOfferingDTO>>> GetAdminStudentMyCourses([FromRoute] string AcdimicID, CancellationToken cancellationToken = default)
        {
              var userId = await GetCurrentUserId(AcdimicID);

                if (userId < 1)
                {
                    return BadRequest("Invalid Request.");
                }

                var courses = await _mediator.Send(new ATLAsBLL.Services.CourseOffering.Queries.GetmyCourse.Student.GetMyCourseQuery(userId), cancellationToken);
                if (courses == null || !courses.Any())
                {
                    return NotFound($"No courses found for student ID {userId}.");
                }
                return Ok(courses);
            
        }



        /// <summary>
        /// Get Staff My Courses.
        /// </summary>
        [HttpGet("Staff/MyCourses", Name = "GetStaffMyCourses")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Doctor,TeachingAssistant")]
        [EnableRateLimiting("CourseQueryPolicy")]
        public async Task<ActionResult<IEnumerable<DoctorCourseOfferingDTO>>> GetStaffMyCourses(CancellationToken cancellationToken = default)
        {
            var StaffId = GetCurrentUserId();
            if (StaffId < 1)
            {
                return BadRequest("Invalid Staff ID.");
            }
            var courses = await _mediator.Send(new ATLAsBLL.Services.CourseOffering.Queries.GetmyCourse.Doctor.GetStaffMyCourseQuery(StaffId), cancellationToken);
            if (courses == null || !courses.Any())
            {
                return NotFound($"No courses found for Staff ID {StaffId}.");
            }
            return Ok(courses);
        }


        /// <summary>
        /// Get Admin Staff My Courses.
        /// </summary>

        [HttpGet("Admin/Staff/MyCourses/{AcdimicID}", Name = "GetAdminStaffMyCourses")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "SAdmin,Admin")]
        [EnableRateLimiting("CourseQueryPolicy")]
        public async Task<ActionResult<IEnumerable<DoctorCourseOfferingDTO>>> GetAdminStaffMyCourses([FromRoute] string AcdimicID, CancellationToken cancellationToken = default)
        {
            
                var StaffId = await GetCurrentUserId(AcdimicID);

                if (StaffId < 1)
                {
                    return BadRequest("Invalid Request.");
                }
                var courses = await _mediator.Send(new ATLAsBLL.Services.CourseOffering.Queries.GetmyCourse.Doctor.GetStaffMyCourseQuery(StaffId), cancellationToken);
                if (courses == null || !courses.Any())
                {
                    return NotFound($"No courses found for Staff ID {StaffId }.");
                }
                return Ok(courses);

        }


        /// <summary>
        /// Get Course Details.
        /// </summary>
        [HttpGet("CourseDetails", Name = "GetCourseDetails")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Student,Admin,SAdmin")]
        [EnableRateLimiting("CourseDetailsPolicy")]
        public async Task<ActionResult<IEnumerable<CourseofferingDetailsDTO>>> GetCourseDetails(int courseOfferingId, CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();
            if (userId < 1 || courseOfferingId < 1)
            {
                return BadRequest("Invalid student ID or course offering ID.");
            }
            var courses = await _mediator.Send(new ATLAsBLL.Services.CourseOffering.Queries.GetCourseDetails.GetCourseDetailsQuery(courseOfferingId, userId), cancellationToken);
            if (courses == null || !courses.Any())
            {
                return NotFound($"No course details found for CourseOfferingId {courseOfferingId} and StudentId {userId}.");
            }
            return Ok(courses);
        }
    




        /// <summary>
        /// Get Doctor IDs.
        /// </summary>
        [HttpGet("DoctorIDs", Name = "GetDoctorIDs")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Student")]
        [EnableRateLimiting("CourseQueryPolicy")]
        public async Task<ActionResult<IEnumerable<int>>> GetDoctorIDs(int courseOfferingId, CancellationToken cancellationToken = default)
        {
            
                var userId = GetCurrentUserId();
                if (userId < 1 || courseOfferingId < 1)
                {
                    return BadRequest("Invalid user ID or course offering ID.");
                }
                var doctorIDs = await _mediator.Send(new ATLAsBLL.Services.CourseOfferingDoctor.Query.GetDoctorIDQuery(courseOfferingId), cancellationToken);
                if (doctorIDs == null || !doctorIDs.Any())
                {
                    return NotFound($"No doctors found for CourseOfferingId {courseOfferingId}.");
                }
                return Ok(doctorIDs);
   
        }
        /// <summary>
        /// Get Current User Id.
        /// </summary>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return default;
            }
            return userId;
        }
        /// <summary>
        /// Get Current User Id.
        /// </summary>
        private async Task<int> GetCurrentUserId(string academicId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(academicId))
            {
                throw new ArgumentException("Academic ID cannot be empty.", nameof(academicId));
            }

            // ✅ استخدام await للحصول على النتيجة كـ int?
            var userID = await _mediator.Send(new GetUserID(academicId), cancellationToken);

            if (userID is null)
            {
                throw new KeyNotFoundException($"User with Academic ID '{academicId}' was not found.");
            }

            return userID.Value;
        }


    }
}
