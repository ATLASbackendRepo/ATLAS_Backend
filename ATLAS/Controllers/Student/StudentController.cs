using ATLAsBLL.Services.Student.Queries.GetallStudent;
using ATLAsBLL.Services.Student.Queries.GetStudentByCode;
using ATLAsBLL.Services.Student.Queries.GetSTudentGrade;
using ATLAsBLL.Services.Student.Queries.GetStudentsByLevel;
using ATLAsBLL.Services.Student.Queries.GetStudentsByOferringCoures;
using ATLAsBLL.Services.Student.Queries.GetStudentWithMode;
using ATLASDomain.DTO.Student;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using ATLAS;

namespace ATLAS.Controllers.Student
{
    [Route("api/Students")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICourseOfferingAuthorizationService _courseAuthorization;

        public StudentController(IMediator mediator, ICourseOfferingAuthorizationService courseAuthorization)
        {
            _mediator = mediator;
            _courseAuthorization = courseAuthorization;
        }

        [HttpGet("All", Name = "GetAllStudent")]
        [Authorize(Roles = "Admin,SAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<StudentDTO>>> GetAllStudent()
        {

            var Students = await _mediator.Send(new GetAllStudentsQuery());
            if (Students == null || (Students.Count() == 0))
            {
                return NotFound("No students found.");
            }
            return Ok(Students);
        }


        [HttpGet("StudentsLevel/{level}", Name = "GetAllStudentWithLevel")]
        [Authorize(Roles = "Admin,SAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<StudentDTO>>> GetAllStudentWithLevel(int level)
        {
            if(level <1 || level >5)
            {
                return BadRequest("Invalid level.");
            }

            var Students = await _mediator.Send(new GetStudentsByLevelQuery(level));
            if (Students == null || (Students.Count() == 0))
            {
                return NotFound("No students found.");
            }
            return Ok(Students);
        }



        [HttpGet("StudentsCourse/{CourseOffringID}", Name = "GetAllStudentWithCourseffering")]
        [Authorize(Roles = "Doctor,TeachingAssistant,Admin,SAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<StudentDTO>>> GetAllStudentWithCourseffering(int CourseOffringID)
        {
            if(CourseOffringID < 1 )
            {
                return BadRequest("Invalid course offering ID.");
            }

            if (!await _courseAuthorization.CanAccessOfferingAsync(User, CourseOffringID, HttpContext.RequestAborted))
                return Forbid();

            var Students = await _mediator.Send(new GetStudentsByOferringCouresQuery(CourseOffringID));
           
            if (Students == null || (Students.Count() == 0))
            {
                return NotFound("No students found.");
            }
            return Ok(Students);
        }


        [HttpGet("StudentsGrade", Name = "GetAllStudentWithModel")]
        [Authorize(Roles = "Doctor,TeachingAssistant,Admin,SAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<StudentGradeDto>>> GetAllStudentWithModel(int CourseOffringID, int modelId)
        {
            if(CourseOffringID < 1 || modelId < 1)
            {
                return BadRequest("Invalid course offering ID or Model ID.");
            }

            if (!await _courseAuthorization.CanAccessOfferingAsync(User, CourseOffringID, HttpContext.RequestAborted))
                return Forbid();

            var Students = await _mediator.Send(new GetStudentWithModequery(CourseOffringID, modelId));
           
            if (Students == null || (Students.Count() == 0))
            {
                return NotFound("No students found in this mode.");
            }
            return Ok(Students);
        }


        [HttpGet("StudentGrade/{StudentCode}", Name = "GetStudentWithModel")]
        [Authorize(Roles = "Student")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<StudentGradeDto>> GetStudentWithModel(int CourseOffringID, int modelId,string StudentCode)
        {
            var authenticatedStudentCode = User.FindFirst("StudentCode")?.Value;
            if (string.IsNullOrWhiteSpace(authenticatedStudentCode))
                return Unauthorized();

            if (!string.Equals(StudentCode, authenticatedStudentCode, StringComparison.Ordinal))
                return Forbid();


            if (CourseOffringID < 1 || modelId < 1)
            {
                return BadRequest("Invalid course offering ID or Model ID.");
            }
            var Students = await _mediator.Send(new GetStudentGradeQuery(CourseOffringID, modelId, StudentCode));
            if (Students == null)
            {
                return NotFound("No students found.");
            }
            return Ok(Students);
        }






    












      


    }
}
