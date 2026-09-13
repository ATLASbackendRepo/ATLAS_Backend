using ATLAS;
using ATLAsBLL.Features.Students.Commands.CreateStudent;
using ATLAsBLL.Features.Students.Commands.CreateStudentsRange;
using ATLAsBLL.Features.Students.Commands.DeleteStudent;
using ATLAsBLL.Features.Students.Commands.UpdateStudent;
using ATLAsBLL.Services.Student.Queries.GetallStudent;
using ATLAsBLL.Services.Student.Queries.GetStudentByCode;
using ATLAsBLL.Services.Student.Queries.GetSTudentGrade;
using ATLAsBLL.Services.Student.Queries.GetStudentsByLevel;
using ATLAsBLL.Services.Student.Queries.GetStudentsByOferringCoures;
using ATLAsBLL.Services.Student.Queries.GetStudentWithMode;
using ATLASDomain.DTO.Student;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

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
        [EnableRateLimiting("AdminGetAllPolicy")]
        public async Task<ActionResult<IEnumerable<StudentDTO>>> GetAllStudent([FromQuery] StudentFilterDTO studentFilter)
        {

            var Students = await _mediator.Send(new GetAllStudentsQuery(studentFilter));
            if (Students == null || (Students.Count() == 0))
            {
                return NotFound("No students found.");
            }
            return Ok(Students);
        }


        [HttpPost("upload")]
        [Authorize(Roles = "SAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [EnableRateLimiting("AdminCreateRangePolicy")]
        public async Task<ActionResult> CreateStudents(IFormFile file)
        {
            try
            {
                const long maxUploadBytes = 10 * 1024 * 1024;
                var extension = Path.GetExtension(file?.FileName ?? string.Empty).ToLowerInvariant();
                if (file == null || file.Length == 0 || file.Length > maxUploadBytes || extension is not ".xlsx" and not ".xls")
                {
                    return BadRequest("Upload a non-empty Excel file no larger than 10 MB.");
                }

                var signature = new byte[8];
                var bytesRead = await file.OpenReadStream().ReadAsync(signature);
                var isXlsx = signature.Take(2).SequenceEqual(new byte[] { 0x50, 0x4B });
                var isXls = signature.Take(bytesRead).SequenceEqual(new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 });
                if ((extension == ".xlsx" && !isXlsx) || (extension == ".xls" && !isXls))
                    return BadRequest("The uploaded file does not match its Excel format.");

                var uploadsFolder = Path.Combine(Path.GetTempPath(), "ATLAS", "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filename = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, filename);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }



                bool insertSecessfully = await _mediator.Send(new CreateStudentsRangeCommand(filePath));

                return Ok(new { message = "File uploaded and data saved successfully." });
            }
            catch
            {
                return BadRequest("invaliddata");


            }
        }

        [HttpPost("CreateStudent")]
        [Authorize(Roles = "SAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [EnableRateLimiting("AdminUpdatePolicy")]
        public async Task<ActionResult> CreateStudent([FromBody] CreateStudentDTO createStudentDTO)
        {
                 var studentId = await _mediator.Send(new CreateStudentCommand(createStudentDTO));
            if(studentId > 0)
            {
                return Ok(new { message = "Student created successfully.", StudentId = studentId });
            }
            else
            {
                return BadRequest("Failed to create student.");
            }

        }
        [HttpPut("UpdateStudent")]
        [Authorize(Roles = "SAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [EnableRateLimiting("AdminUpdatePolicy")]
        public async Task<ActionResult> updateStudent([FromBody] UpdateStudentDto UpdateStudentDTO)
        {
                 var student = await _mediator.Send(new UpdateStudentCommand( UpdateStudentDTO));
            if(  student )
            {
                return Ok(new { message = "Student updated successfully." });
            }
            else
            {
                return BadRequest("Failed to update student.");
            }

        }
        [HttpDelete("DeleteStudent")]
        [Authorize(Roles = "SAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [EnableRateLimiting("AdminSoftDeletePolicy")]
        public async Task<ActionResult> deleteStudent([FromQuery] string StudentCode)
        {
                 var student = await _mediator.Send(new DeleteStudentCommand(StudentCode));
            if(  student )
            {
                return Ok(new { message = "Student deleted successfully." });
            }
            else
            {
                return BadRequest("Failed to delete student.");
            }

        }


















        //[HttpGet("StudentsLevel/{level}", Name = "GetAllStudentWithLevel")]
        //[Authorize(Roles = "Admin,SAdmin")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //public async Task<ActionResult<IEnumerable<StudentDTO>>> GetAllStudentWithLevel(int level)
        //{
        //    if(level <1 || level >5)
        //    {
        //        return BadRequest("Invalid level.");
        //    }

        //    var Students = await _mediator.Send(new GetStudentsByLevelQuery(level));
        //    if (Students == null || (Students.Count() == 0))
        //    {
        //        return NotFound("No students found.");
        //    }
        //    return Ok(Students);
        //}



        //[HttpGet("StudentsCourse/{CourseOffringID}", Name = "GetAllStudentWithCourseffering")]
        //[Authorize(Roles = "Doctor,TeachingAssistant,Admin,SAdmin")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //public async Task<ActionResult<IEnumerable<StudentDTO>>> GetAllStudentWithCourseffering(int CourseOffringID)
        //{
        //    if(CourseOffringID < 1 )
        //    {
        //        return BadRequest("Invalid course offering ID.");
        //    }

        //    if (!await _courseAuthorization.CanAccessOfferingAsync(User, CourseOffringID, HttpContext.RequestAborted))
        //        return Forbid();

        //    var Students = await _mediator.Send(new GetStudentsByOferringCouresQuery(CourseOffringID));
           
        //    if (Students == null || (Students.Count() == 0))
        //    {
        //        return NotFound("No students found.");
        //    }
        //    return Ok(Students);
        //}


        //[HttpGet("StudentsGrade", Name = "GetAllStudentWithModel")]
        //[Authorize(Roles = "Doctor,TeachingAssistant,Admin,SAdmin")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //public async Task<ActionResult<IEnumerable<StudentGradeDto>>> GetAllStudentWithModel(int CourseOffringID, int modelId)
        //{
        //    if(CourseOffringID < 1 || modelId < 1)
        //    {
        //        return BadRequest("Invalid course offering ID or Model ID.");
        //    }

        //    if (!await _courseAuthorization.CanAccessOfferingAsync(User, CourseOffringID, HttpContext.RequestAborted))
        //        return Forbid();

        //    var Students = await _mediator.Send(new GetStudentWithModequery(CourseOffringID, modelId));
           
        //    if (Students == null || (Students.Count() == 0))
        //    {
        //        return NotFound("No students found in this mode.");
        //    }
        //    return Ok(Students);
        //}


        [HttpGet("MyGrade", Name = "GetMyGrade")]
        [Authorize(Roles = "Student")]
        [ProducesResponseType(typeof(StudentGradeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<StudentGradeDto>> GetMyGrade(int courseOfferingId, int moduleId, CancellationToken cancellationToken)
        {
            if (courseOfferingId < 1 || moduleId < 1)
                return BadRequest("Course offering ID and module ID must be greater than zero.");

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var grade = await _mediator.Send(
                new GetStudentGradeForUserQuery(userId, courseOfferingId, moduleId), cancellationToken);

            return grade is null ? NotFound() : Ok(grade);
        }
    }
}
