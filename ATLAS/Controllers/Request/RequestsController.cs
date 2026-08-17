using ATLAsBLL.Services.Request.Command.CreateGradeAppealRequest;
using ATLAsBLL.Services.Request.Command.UpdateGradeAppealRequest;
using ATLAsBLL.Services.Request.Qeuries;
using ATLASDomain.DTO.Request;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ATLAS.Controllers.Requests;

    [ApiController]
[Route("api/Request")]
public class RequestController : ControllerBase
{
    private readonly IMediator _mediator;

    public RequestController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreateGradeAppealRequestDto dto,
        CancellationToken cancellationToken)
    {
        // Your JWT must contain the StudentId claim.
        var studentIdValue = User.FindFirstValue("StudentId");

        if (!int.TryParse(studentIdValue, out int studentId))
            return Unauthorized("Student identity was not found.");

        var requestId = await _mediator.Send(
            new CreateGradeAppealRequestCommand(studentId, dto),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { requestId },
            new { requestId });
    }

    [HttpPatch("{requestId:int}")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(
        int requestId,
        [FromBody] UpdateGradeAppealRequestDto dto,
        CancellationToken cancellationToken)
    {
        // Standard JWT user identifier claim.
        var doctorUserIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(doctorUserIdValue, out int doctorUserId))
            return Unauthorized("Doctor identity was not found.");

        var updated = await _mediator.Send(
            new UpdateGradeAppealRequestCommand(requestId, doctorUserId, dto),
            cancellationToken);

        return updated ? NoContent() : NotFound();
    }

    [HttpGet("student")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<GradeAppealRequestDto>>> GetMyRequests(
        CancellationToken cancellationToken)
    {
        var studentIdValue = User.FindFirstValue("StudentId");

        if (!int.TryParse(studentIdValue, out int studentId))
            return Unauthorized();

        var requests = await _mediator.Send(
            new GetStudentGradeAppealsQuery(studentId),
            cancellationToken);

        return Ok(requests);
    }

    [HttpGet("doctor")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
    [Authorize(Roles = "Student,Doctor,SAdmin")]
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
}
