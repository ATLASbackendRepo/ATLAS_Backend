using ATLAsBLL.Exceptions;
using ATLAsBLL.Services.Notifications.Command.MarkAllNotificationsAsRead;
using ATLAsBLL.Services.Notifications.Command.MarkNotificationAsRead;
using ATLAsBLL.Services.Notifications.DTOs;
using ATLAsBLL.Services.Notifications.Queries.GetMyNotifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace ATLAS.Controllers;


[Authorize]
[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

  
    [HttpGet ( Name = "GetNotifications" )]
    [ProducesResponseType(typeof(List<NotificationDto>), 200)]
    [ProducesResponseType(statusCode: 404)]
    [Authorize(Roles = "Doctor,TeachingAssistant,Student")]
    [EnableRateLimiting("NotificationsPolicy")]

    public async Task<ActionResult<List<NotificationDto>>> GetNotifications(
        [FromQuery] bool unreadOnly = false,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var result = await _mediator.Send(new GetMyNotificationsQuery(userId, unreadOnly), cancellationToken);
        if (result == null)
        {
            return NotFound(new { message = "No notifications found." });
        }
        return Ok(result);
    }

    /// <summary>
    /// Mark a single notification as read.
    /// </summary>
    [HttpPost("{notificationId}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Doctor,TeachingAssistant,Student")]
    [EnableRateLimiting("NotificationsPolicy")]
    public async Task<IActionResult> MarkAsRead(
    [FromRoute] int notificationId,
    CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var command = new MarkNotificationAsReadCommand(notificationId, userId);
            await _mediator.Send(command, cancellationToken);
            return Ok(new { notificationId, message = "Notification marked as read." });
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
    /// Mark all notifications as read for the current user.
    /// </summary>
    [HttpPost("read-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize(Roles = "Doctor,TeachingAssistant,Student")]
    [EnableRateLimiting("NotificationsReadAllPolicy")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var command = new MarkAllNotificationsAsReadCommand(userId);
            var count = await _mediator.Send(command, cancellationToken);
            return Ok(new { count, message = "All notifications marked as read." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private int     GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token.");
        }
        return userId;
    }
}
