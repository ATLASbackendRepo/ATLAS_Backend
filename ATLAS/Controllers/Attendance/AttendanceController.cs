using System.Security.Claims;
using ATLAS.Attendance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATLAS.Controllers.Attendance;

[ApiController]
[Route("api/attendance")]
public sealed class AttendanceController(IAttendanceService attendance, ICourseOfferingAuthorizationService offeringAuthorization) : ControllerBase
{
    [HttpPost("sessions")]
    [Authorize(Roles = "Doctor,TeachingAssistant,Admin,SAdmin")]
    [ProducesResponseType(typeof(AttendanceSessionDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<AttendanceSessionDto>> Start([FromBody] StartAttendanceSessionRequest request, CancellationToken cancellationToken)
    {
        if (request.CourseOfferingId <= 0 || request.DurationMinutes is < 1 or > 480) return BadRequest(new { message = "CourseOfferingId and a duration between 1 and 480 minutes are required." });
        if (!await offeringAuthorization.CanAccessOfferingAsync(User, request.CourseOfferingId, cancellationToken)) return Forbid();
        if (!TryGetUserId(out var userId)) return Unauthorized();
        try
        {
            var session = await attendance.StartAsync(request.CourseOfferingId, userId, request.DurationMinutes, cancellationToken);
            return CreatedAtAction(nameof(GetSession), new { sessionId = session.SessionId }, session);
        }
        catch (AttendanceConflictException ex) { return Conflict(new { message = ex.Message }); }
        catch (AttendanceValidationException ex) { return NotFound(new { message = ex.Message }); }
    }


    [HttpGet("sessions/{sessionId:guid}")]
    [Authorize(Roles = "Doctor,TeachingAssistant,Admin,SAdmin")]
    public async Task<ActionResult<AttendanceSessionDto>> GetSession(Guid sessionId, CancellationToken cancellationToken)
    {
        var report = await attendance.GetReportAsync(sessionId, cancellationToken);
        if (report is null) return NotFound();
        if (!await offeringAuthorization.CanAccessOfferingAsync(User, report.CourseOfferingId, cancellationToken)) return Forbid();
        return Ok(new AttendanceSessionDto(report.SessionId, report.CourseOfferingId, report.StartedAt, report.ExpiresAt, report.EndedAt, report.Status, report.Students.Count(x => x.Status == "Present")));
    }

    
    [HttpGet("sessions/{sessionId:guid}/qr")]
    [Authorize(Roles = "Doctor,TeachingAssistant,Admin,SAdmin")]
    [ProducesResponseType(typeof(AttendanceQrDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AttendanceQrDto>> GetQr(Guid sessionId, CancellationToken cancellationToken)
    {
        var report = await attendance.GetReportAsync(sessionId, cancellationToken);
        if (report is null) return NotFound();
        if (!await offeringAuthorization.CanAccessOfferingAsync(User, report.CourseOfferingId, cancellationToken)) return Forbid();
        var qr = await attendance.GetCurrentQrAsync(sessionId, cancellationToken);
        return qr is null ? Conflict(new { message = "This attendance session is no longer active." }) : Ok(qr);
    }

    [HttpPost("scan")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(AttendanceScanResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<AttendanceScanResult>> Scan([FromBody] AttendanceScanRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        if (string.IsNullOrWhiteSpace(request.QrToken)) return BadRequest(new { message = "A QR token is required." });
        try { return Ok(await attendance.RecordScanAsync(request.QrToken, userId, request.DeviceId, cancellationToken)); }
        catch (AttendanceValidationException ex) { return BadRequest(new { message = ex.Message }); }
        catch (AttendanceConflictException ex) { return Conflict(new { message = ex.Message }); }
    }


    [HttpPost("sessions/{sessionId:guid}/end")]
    [Authorize(Roles = "Doctor,TeachingAssistant,Admin,SAdmin")]
    public async Task<ActionResult<AttendanceSessionDto>> End(Guid sessionId, CancellationToken cancellationToken)
    {
        var report = await attendance.GetReportAsync(sessionId, cancellationToken);
        if (report is null) return NotFound();
        if (!await offeringAuthorization.CanAccessOfferingAsync(User, report.CourseOfferingId, cancellationToken)) return Forbid();
        return Ok(await attendance.EndAsync(sessionId, cancellationToken));
    }

    [HttpGet("sessions/{sessionId:guid}/report")]
    [Authorize(Roles = "Doctor,TeachingAssistant,Admin,SAdmin")]
    [ProducesResponseType(typeof(AttendanceReportDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AttendanceReportDto>> Report(Guid sessionId, CancellationToken cancellationToken)
    {
        var report = await attendance.GetReportAsync(sessionId, cancellationToken);
        if (report is null) return NotFound();
        if (!await offeringAuthorization.CanAccessOfferingAsync(User, report.CourseOfferingId, cancellationToken)) return Forbid();
        return Ok(report);
    }

    [HttpGet("{sessionId:guid}/export/csv")]
    [Authorize(Roles = "Doctor,TeachingAssistant,Admin,SAdmin")]
    [ResponseCache(NoStore = true)]
    public async Task<IActionResult> ExportCsv(
    Guid sessionId,
    CancellationToken cancellationToken)
    {
        var report = await attendance.GetReportAsync(
            sessionId,
            cancellationToken);

        if (report is null)
            return NotFound();
        if (!await offeringAuthorization.CanAccessOfferingAsync(User, report.CourseOfferingId, cancellationToken)) return Forbid();

        Response.ContentType = "text/csv; charset=utf-8";
        Response.Headers.ContentDisposition =
            $"attachment; filename=\"attendance-{sessionId}.csv\"";

        await AttendanceReportExporter.WriteCsvAsync(
            report,
            Response.Body,
            cancellationToken);

        return new EmptyResult();
    }

    [HttpGet("{sessionId:guid}/export/pdf")]
    [Authorize(Roles = "Doctor,TeachingAssistant,Admin,SAdmin")]
    [ResponseCache(NoStore = true)]
    public async Task<IActionResult> ExportPdf(
    Guid sessionId,
    CancellationToken cancellationToken)
    {
        var report = await attendance.GetReportAsync(
            sessionId,
            cancellationToken);

        if (report is null)
            return NotFound();
        if (!await offeringAuthorization.CanAccessOfferingAsync(User, report.CourseOfferingId, cancellationToken)) return Forbid();

        await using var stream = new MemoryStream();

        AttendanceReportExporter.WritePdf(report, stream);

        return File(
            stream.ToArray(),
            "application/pdf",
            $"attendance-{sessionId}.pdf");
    }

    private bool TryGetUserId(out int userId) => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}
