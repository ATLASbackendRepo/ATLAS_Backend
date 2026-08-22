using ATLAsBLL.Services.CourseGrade.Queries;
using ATLAsBLL.Services.CourseGrade.Queries.GetMySemesterRank;
using ATLASDomain.DTO.CourseGrade;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

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
    [ProducesResponseType(typeof(MyRankDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "Student")]
    [EnableRateLimiting("RankingsPolicy")]
    public async Task<ActionResult<MyRankDto>> GetMyRank(
    int semesterId, 
    CancellationToken cancellationToken)
    {
     

        if (semesterId < 1)
            return BadRequest("SemesterId must be greater than zero.");

         var Userid= GetCurrentUserId();

        
        var result = await _mediator.Send(
            new GetMySemesterRankQuery(Userid, semesterId),
            cancellationToken);

        if (result is null)
            return NotFound(
                $"Student  has no enrollments in  this Semester .");

        return Ok(result);
    }

    /// <summary>
    /// GET /api/semesters/3/rankings/top/3
    /// GET /api/semesters/3/rankings/top/10
    /// </summary>
    [HttpGet("top/{count:int}")]
    [ProducesResponseType(typeof(List<TopRankedStudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(List<TopRankedStudentDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(List<TopRankedStudentDto>), StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Student")]
    [EnableRateLimiting("RankingsPolicy")]
    public async Task<ActionResult<List<TopRankedStudentDto>>> GetTopRanked(   // forget level (int)
        int semesterId,
        int AcademicLevel,
        int count,
        CancellationToken cancellationToken)
    {
        if (semesterId < 1)
            return BadRequest("SemesterId must be greater than zero.");

        if (AcademicLevel < 1)
            return BadRequest("AcademicLevel must be greater than zero.");

        if (count <= 0)
            return BadRequest("Count must be greater than zero.");

        var result = await _mediator.Send(
            new GetTopRankedStudentsInSemesterQuery(semesterId, AcademicLevel, count), cancellationToken);

        if (result is null || result.Count == 0)
            return NotFound($"No ranked students found in Semester {semesterId}.");

        return Ok(result);
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
