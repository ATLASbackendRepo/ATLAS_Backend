using System.Security.Claims;
using ATLAS.Attendance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ATLAS.Controllers.Attendance;
/// <summary>
/// Attendance Controller.
/// </summary>

[ApiController]
[Route("api/attendance")]
public sealed class AttendanceController(IAttendanceService attendance, IDeviceBindingService deviceBinding, ICourseOfferingAuthorizationService offeringAuthorization) : ControllerBase
{
    /// <summary>
    /// Start.
    /// </summary>
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


    /// <summary>
    /// Get Session.
    /// </summary>
    [HttpGet("sessions/{sessionId:guid}")]
    [Authorize(Roles = "Doctor,TeachingAssistant,Admin,SAdmin")]
    public async Task<ActionResult<AttendanceSessionDto>> GetSession(Guid sessionId, CancellationToken cancellationToken)
    {
        var report = await attendance.GetReportAsync(sessionId, cancellationToken);
        if (report is null) return NotFound();
        if (!await offeringAuthorization.CanAccessOfferingAsync(User, report.CourseOfferingId, cancellationToken)) return Forbid();
        return Ok(new AttendanceSessionDto(report.SessionId, report.CourseOfferingId, report.StartedAt, report.ExpiresAt, report.EndedAt, report.Status, report.Students.Count(x => x.Status == "Present")));
    }

    
    /// <summary>
    /// Get Qr.
    /// </summary>
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

    /// <summary>
    /// Scan.
    /// </summary>
    [HttpPost("scan")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(AttendanceScanResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<AttendanceScanResult>> Scan([FromBody] AttendanceScanRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        if (string.IsNullOrWhiteSpace(request.QrToken)) return BadRequest(new { message = "A QR token is required." });
        try { return Ok(await attendance.RecordScanAsync(request, userId, cancellationToken)); }
        catch (AttendanceValidationException ex) { return BadRequest(new { message = ex.Message }); }
        catch (AttendanceConflictException ex) { return Conflict(new { message = ex.Message }); }
    }

    /// <summary>
    /// Pair Device.
    /// </summary>
    /// <remarks>
    /// MED-17: the only way a device becomes able to record attendance. The server
    /// mints the secret and the pairing token here, so a client that invents a
    /// device identifier has nothing a scan can be signed with.
    /// </remarks>
    [HttpPost("devices")]
    [Authorize(Roles = "Student")]
    [EnableRateLimiting("DevicePairPolicy")]
    [ProducesResponseType(typeof(DevicePairingDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<DevicePairingDto>> PairDevice([FromBody] DevicePairingRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        if (string.IsNullOrWhiteSpace(request.DeviceId)) return BadRequest(new { message = "A device identifier is required." });
        try
        {
            var pairing = await deviceBinding.PairAsync(userId, request.DeviceId, request.DeviceName, cancellationToken);
            return CreatedAtAction(nameof(ListDevices), null, pairing);
        }
        catch (AttendanceValidationException ex) { return BadRequest(new { message = ex.Message }); }
        catch (AttendanceConflictException ex) { return Conflict(new { message = ex.Message }); }
    }

    /// <summary>
    /// List Devices.
    /// </summary>
    [HttpGet("devices")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(IReadOnlyList<RegisteredDeviceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RegisteredDeviceDto>>> ListDevices(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        return Ok(await deviceBinding.ListAsync(userId, cancellationToken));
    }

    /// <summary>
    /// Revoke Device.
    /// </summary>
    [HttpDelete("devices/{deviceId}")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> RevokeDevice(string deviceId, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        return await deviceBinding.RevokeAsync(userId, deviceId, cancellationToken)
            ? NoContent()
            : NotFound(new { message = "That device is not registered to this account." });
    }


    /// <summary>
    /// End.
    /// </summary>
    [HttpPost("sessions/{sessionId:guid}/end")]
    [Authorize(Roles = "Doctor,TeachingAssistant,Admin,SAdmin")]
    public async Task<ActionResult<AttendanceSessionDto>> End(Guid sessionId, CancellationToken cancellationToken)
    {
        var report = await attendance.GetReportAsync(sessionId, cancellationToken);
        if (report is null) return NotFound();
        if (!await offeringAuthorization.CanAccessOfferingAsync(User, report.CourseOfferingId, cancellationToken)) return Forbid();
        return Ok(await attendance.EndAsync(sessionId, cancellationToken));
    }

    /// <summary>
    /// Report.
    /// </summary>
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

    /// <summary>
    /// Export Csv.
    /// </summary>
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

    /// <summary>
    /// Export Pdf.
    /// </summary>
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

    /// <summary>
    /// Try Get User Id.
    /// </summary>
    private bool TryGetUserId(out int userId) => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}
