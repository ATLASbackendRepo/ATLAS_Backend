using ATLAsBLL.Services.CourseGrade.Queries;
using ATLAsBLL.Services.CourseGrade.Queries.GetMySemesterRank;
using ATLASDomain.DTO.CourseGrade;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ATLAS.API.Controllers;

[ApiController]
[Route("api/semesters/{semesterId:int}/rankings")]
public class RankingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RankingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// GET /api/semesters/3/rankings/me?studentId=12
    /// </summary>
    [HttpGet("me")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(MyRankDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MyRankDto>> GetMyRank(
        int semesterId,
        [FromQuery] int studentId,
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirst("StudentId")?.Value, out var authenticatedStudentId))
            return Unauthorized();

        if (studentId != authenticatedStudentId)
            return Forbid();

        if (studentId < 1)
            return BadRequest("StudentId must be greater than zero.");

        if (semesterId < 1)
            return BadRequest("SemesterId must be greater than zero.");

        var result = await _mediator.Send(
            new GetMySemesterRankQuery(studentId, semesterId), cancellationToken);

        if (result is null)
            return NotFound($"Student {studentId} has no enrollments in Semester {semesterId}.");

        return Ok(result);
    }

    /// <summary>
    /// GET /api/semesters/3/rankings/top/3
    /// GET /api/semesters/3/rankings/top/10
    /// </summary>
    [HttpGet("top/{count:int}")]
    [Authorize(Roles = "Admin,SAdmin")]
    [ProducesResponseType(typeof(List<TopRankedStudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(List<TopRankedStudentDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(List<TopRankedStudentDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<TopRankedStudentDto>>> GetTopRanked(   // forget level (int)
        int semesterId,
        int count,
        CancellationToken cancellationToken)
    {
        if (semesterId < 1)
            return BadRequest("SemesterId must be greater than zero.");

        if (count <= 0)
            return BadRequest("Count must be greater than zero.");

        var result = await _mediator.Send(
            new GetTopRankedStudentsInSemesterQuery(semesterId, count), cancellationToken);

        if (result is null || result.Count == 0)
            return NotFound($"No ranked students found in Semester {semesterId}.");

        return Ok(result);
    }
}
