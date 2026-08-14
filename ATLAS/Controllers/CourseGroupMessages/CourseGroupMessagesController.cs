using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using System.Security.Claims;
using ATLAsBLL.Services.CourseGroupMessage.Command.SendCourseGroupMessage;
using ATLAsBLL.Services.CourseGroupMessage.Command.EditCourseGroupMessage;
using ATLAsBLL.Services.CourseGroupMessage.Command.DeleteCourseGroupMessage;
using ATLAsBLL.Services.CourseGroupMessage.Command.TogglePinCourseGroupMessage;
using ATLAsBLL.Services.CourseGroupMessage.Command.MarkMessageAsRead;
using ATLAsBLL.Services.CourseGroupMessage.Queries.GetCourseGroupMessages;
using ATLAsBLL.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace ATLAS.Controllers;

/// <summary>
/// Manages course group messaging, including sending, editing, and reading messages.
/// Authorization: Doctors/TAs can send/edit/pin; Students are read-only.
/// </summary>
[Authorize]
[ApiController]
[Route("api/course-offerings/{courseOfferingId}/messages")]
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
    [HttpGet]
    public async Task<IActionResult> GetMessages(
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
    [HttpPost]
    public async Task<IActionResult> SendMessage(
        [FromRoute] int courseOfferingId,
        [FromBody] SendMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var command = new SendCourseGroupMessageCommand(courseOfferingId, userId, request.Content);
            var messageId = await _mediator.Send(command, cancellationToken);
            return Created($"/api/course-offerings/{courseOfferingId}/messages/{messageId}", new { messageId });
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
    [HttpPut("{messageId}")]
    public async Task<IActionResult> EditMessage(
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
            return Ok(new { messageId, message = "Message updated successfully." });
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
    [HttpDelete("{messageId}")]
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
            return Ok(new { messageId, message = "Message deleted successfully." });
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
    [HttpPost("{messageId}/pin")]
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
            return Ok(new { messageId, message = "Pin status toggled successfully." });
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
    [HttpPost("{messageId}/read")]
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
            return Ok(new { messageId, message = "Message marked as read." });
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
