using ATLAsBLL.Exceptions;
using ATLAsBLL.Services.CourseGroupMessage.Command.DeleteCourseGroupMessage;
using ATLAsBLL.Services.CourseGroupMessage.Command.EditCourseGroupMessage;
using ATLAsBLL.Services.CourseGroupMessage.Command.MarkMessageAsRead;
using ATLAsBLL.Services.CourseGroupMessage.Command.SendCourseGroupMessage;
using ATLAsBLL.Services.CourseGroupMessage.Command.TogglePinCourseGroupMessage;
using ATLAsBLL.Services.CourseGroupMessage.DTOs;
using ATLAsBLL.Services.CourseGroupMessage.Queries.GetCourseGroupMessages;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace ATLAS.Controllers;

/// <summary>
/// Manages course group messaging, including sending, editing, and reading messages.
/// Authorization: Doctors/TAs can send/edit/pin; Students are read-only.
/// </summary>
[Authorize]
[ApiController]
[Route("api/course/{courseOfferingId}/messages")]
public class CourseGroupMessagesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CourseGroupMessagesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get paginated list of messages in a course offering.
    /// Authorization: User must be enrolled student or assigned staff (Doctor/TA).
    /// </summary>
    [HttpGet(Name = "GetMessages")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "Doctor,TeachingAssistant,Student")]
    public async Task<ActionResult<List<CourseGroupMessageDto>>> GetMessages(
        [FromRoute] int courseOfferingId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var query = new GetCourseGroupMessagesQuery(courseOfferingId, userId, page, pageSize);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (ForbiddenAccessException ex)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Send a new message to a course group.
    /// Authorization: Only Doctors/TAs assigned to the course can send.
    /// </summary>
    [HttpPost(Name = "Send")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "Doctor,TeachingAssistant,Student")]
    public async Task<ActionResult<int>> SendMessage(
        [FromRoute] int courseOfferingId,
        [FromBody] SendMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var command = new SendCourseGroupMessageCommand(courseOfferingId, userId, request.Content);
            var messageId = await _mediator.Send(command, cancellationToken);
            return Created();
        }
        catch (ForbiddenAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { errors = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Edit an existing message (sender only).
    /// </summary>
    [HttpPut("{messageId}", Name = "Edit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "Doctor,TeachingAssistant,Student")]
    public async Task<ActionResult<int>> EditMessage(
        [FromRoute] int courseOfferingId,
        [FromRoute] int messageId,
        [FromBody] EditMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var command = new EditCourseGroupMessageCommand(messageId, userId, request.NewContent);
            await _mediator.Send(command, cancellationToken);
            return Ok();
        }
        catch (ForbiddenAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Soft-delete a message (sender only).
    /// </summary>
    [HttpDelete("{messageId}", Name = "Delete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "Doctor,TeachingAssistant,Student")]
    public async Task<IActionResult> DeleteMessage(
        [FromRoute] int courseOfferingId,
        [FromRoute] int messageId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var command = new DeleteCourseGroupMessageCommand(messageId, userId);
            await _mediator.Send(command, cancellationToken);
            return Ok();
        }
        catch (ForbiddenAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Toggle pin status of a message (sender only).
    /// </summary>
    [HttpPost("{messageId}/pin", Name = "TogglePin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "Doctor,TeachingAssistant,Student")]
    public async Task<IActionResult> TogglePin(
        [FromRoute] int courseOfferingId,
        [FromRoute] int messageId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var command = new TogglePinCourseGroupMessageCommand(messageId, userId);
            await _mediator.Send(command, cancellationToken);
            return Ok();
        }
        catch (ForbiddenAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Mark a message as read by the current student.
    /// </summary>
    [HttpPost("{messageId}/read", Name = "MarkAsRead")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "Doctor,TeachingAssistant,Student")]
    public async Task<IActionResult> MarkAsRead(
        [FromRoute] int courseOfferingId,
        [FromRoute] int messageId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var command = new MarkMessageAsReadCommand(messageId, userId);
            await _mediator.Send(command, cancellationToken);
            return Ok();
        }
        catch (ForbiddenAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
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

public record SendMessageRequest(string Content);
public record EditMessageRequest(string NewContent);
