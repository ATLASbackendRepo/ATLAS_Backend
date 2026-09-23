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




        /// <summary>
        /// Get Admin Student My Courses.
        /// </summary>
        [HttpGet("Admin/Student/MyCourses/{AcdimicID}", Name = "GetAdminStudentMyCourses")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "SAdmin,Admin")]
        [EnableRateLimiting("CourseQueryPolicy")]
        public async Task<ActionResult<IEnumerable<CourseOfferingDTO>>> GetAdminStudentMyCourses([FromRoute] string AcdimicID)
        {
            try
            {
                var userId = await GetCurrentUserId(AcdimicID);

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
            catch (UnauthorizedAccessException ex)
            {
                _logger?.LogWarning(ex, "Unauthorized access in GetAdminStudentMyCourses for AcdimicID {AcdimicID}", AcdimicID);
                return Unauthorized(new { message = "Authentication is required to perform this operation." });
            }
            catch (ArgumentException ex)
            {
                _logger?.LogWarning(ex, "Bad request in GetAdminStudentMyCourses for AcdimicID {AcdimicID}", AcdimicID);
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger?.LogInformation(ex, "User not found in GetAdminStudentMyCourses for AcdimicID {AcdimicID}", AcdimicID);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error in GetAdminStudentMyCourses for AcdimicID {AcdimicID}", AcdimicID);
                return Problem(statusCode: StatusCodes.Status500InternalServerError, title: "An unexpected error occurred.", detail: $"TraceId: {HttpContext?.TraceIdentifier}");
            }
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


        /// <summary>
        /// Get Admin Staff My Courses.
        /// </summary>

        [HttpGet("Admin/Staff/MyCourses/{AcdimicID}", Name = "GetAdminStaffMyCourses")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "SAdmin,Admin")]
        [EnableRateLimiting("CourseQueryPolicy")]
        public async Task<ActionResult<IEnumerable<DoctorCourseOfferingDTO>>> GetAdminStaffMyCourses([FromRoute] string AcdimicID)
        {
            try
            {
                var StaffId = await GetCurrentUserId(AcdimicID);

                if (StaffId < 1)
                {
                    return BadRequest("Invalid Request.");
                }
                var courses = await _mediator.Send(new ATLAsBLL.Services.CourseOffering.Queries.GetmyCourse.Doctor.GetStaffMyCourseQuery(StaffId));
                if (courses == null || !courses.Any())
                {
                    return NotFound($"No courses found for Staff ID {StaffId }.");
                }
                return Ok(courses);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger?.LogWarning(ex, "Unauthorized access in GetAdminStaffMyCourses for AcdimicID {AcdimicID}", AcdimicID);
                return Unauthorized(new { message = "Authentication is required to perform this operation." });
            }
            catch (ArgumentException ex)
            {
                _logger?.LogWarning(ex, "Bad request in GetAdminStaffMyCourses for AcdimicID {AcdimicID}", AcdimicID);
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger?.LogInformation(ex, "User not found in GetAdminStaffMyCourses for AcdimicID {AcdimicID}", AcdimicID);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error in GetAdminStaffMyCourses for AcdimicID {AcdimicID}", AcdimicID);
                return Problem(statusCode: StatusCodes.Status500InternalServerError, title: "An unexpected error occurred.", detail: $"TraceId: {HttpContext?.TraceIdentifier}");
            }
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
    




        /// <summary>
        /// Get Doctor IDs.
        /// </summary>
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
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Authentication is required to perform this operation." });
            }
            catch (Exception)
            {
                return Problem(statusCode: StatusCodes.Status500InternalServerError, title: "An unexpected error occurred.");
            }
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
        /// Get Current User Id.
        /// </summary>
        private async Task<int> GetCurrentUserId(string academicId)
        {
            if (string.IsNullOrWhiteSpace(academicId))
            {
                throw new ArgumentException("Academic ID cannot be empty.", nameof(academicId));
            }

            // ✅ استخدام await للحصول على النتيجة كـ int?
            var userID = await _mediator.Send(new GetUserID(academicId));

            if (userID is null)
            {
                throw new KeyNotFoundException($"User with Academic ID '{academicId}' was not found.");
            }

            return userID.Value;
        }


    }
}
