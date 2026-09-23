using ATLAsBLL.Exceptions;
using ATLAsBLL.Services.Request.Command.CreateGradeAppealRequest;
using ATLAsBLL.Services.Request.Command.UpdateGradeAppealRequest;
using ATLAsBLL.Services.Request.Qeuries;
using ATLASDomain.DTO.Request;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace ATLAS.Controllers.Requests;

    [ApiController]
[Route("api/Request")]
/// <summary>
/// Request Controller.
/// </summary>
public class RequestController : ControllerBase
{
    /// <summary>
    /// _mediator.
    /// </summary>
    private readonly IMediator _mediator;

    public RequestController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize(Roles = "Student")]
    [EnableRateLimiting("RequestWritePolicy")]
    /// <summary>
    /// Create.
    /// </summary>
    public async Task<IActionResult> Create(
        [FromBody] CreateGradeAppealRequestDto dto,
        CancellationToken cancellationToken)
    {
        try
        { 
        var UserId = GetCurrentUserId();


        var requestId = await _mediator.Send(
            new CreateGradeAppealRequestCommand(UserId, dto),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { requestId },
            new { requestId });
         }
        catch (ForbiddenAccessException)
        {
            return Forbid();
}
        catch (Exception)
        {
            return Problem(statusCode: StatusCodes.Status500InternalServerError, title: "An unexpected error occurred.");
        }
    }


    [HttpPatch("{requestId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize(Roles = "Doctor")]
    [EnableRateLimiting("RequestWritePolicy")]
    /// <summary>
    /// Update.
    /// </summary>
    public async Task<IActionResult> Update(
        int requestId,
        [FromBody] UpdateGradeAppealRequestDto dto,
        CancellationToken cancellationToken)
    {
        // Standard JWT user identifier claim.
        var doctorUserId = GetCurrentUserId();

        var updated = await _mediator.Send(
            new UpdateGradeAppealRequestCommand(requestId, doctorUserId, dto),
            cancellationToken);

        return updated ? NoContent() : NotFound();
    }


    [HttpGet("student")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize(Roles = "Student")]
    [EnableRateLimiting("RequestReadPolicy")]
    /// <summary>
    /// Get My Requests.
    /// </summary>
    public async Task<ActionResult<List<GradeAppealRequestDto>>> GetMyRequests(
        CancellationToken cancellationToken)
    {
        var UserId = GetCurrentUserId();



        var requests = await _mediator.Send(
            new GetStudentGradeAppealsQuery(UserId),
            cancellationToken);

        return Ok(requests);
    }


    [HttpGet("doctor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize(Roles = "Doctor")]
    [EnableRateLimiting("RequestReadPolicy")]

    /// <summary>
    /// Get Doctor Requests.
    /// </summary>
    public async Task<ActionResult<List<GradeAppealRequestDto>>> GetDoctorRequests(
        CancellationToken cancellationToken)
    {
        var doctorUserIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(doctorUserIdValue, out int doctorUserId))
            return Unauthorized();

        var requests = await _mediator.Send(
            new GetDoctorGradeAppealsQuery(doctorUserId),
            cancellationToken);

        return Ok(requests);
    }



    [HttpGet("{requestId:int}")]
    [Authorize(Roles = "Student")]
    /// <summary>
    /// Get By Id.
    /// </summary>
    public async Task<ActionResult<GradeAppealRequestDto>> GetById(
    int requestId,
    CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var studentIdValue = User.FindFirstValue("StudentId");

        if (!int.TryParse(userIdValue, out int currentUserId))
            return Unauthorized();

        int? currentStudentId = int.TryParse(studentIdValue, out int studentId)
            ? studentId
            : null;

        var request = await _mediator.Send(
            new GetGradeAppealByIdQuery(
                requestId,
                currentUserId,
                currentStudentId,
                User.IsInRole("SAdmin")),
            cancellationToken);

        return request is null ? NotFound() : Ok(request);
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
}
