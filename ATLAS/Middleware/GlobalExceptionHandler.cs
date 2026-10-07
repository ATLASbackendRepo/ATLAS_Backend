using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ATLAS.Middleware;

/// <summary>
/// Translates unhandled exceptions into a single, consistent problem response.
/// Clients can rely on the status code: their own mistakes are reported as 4xx
/// and only genuine faults become 500, which keeps the error rate meaningful in
/// monitoring. No stack trace or SQL detail is exposed in Production.
/// </summary>
/// <param name="logger">
/// Logger used for the single place where a request failure is recorded.
/// </param>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    /// <summary>
    /// SQL Server unique index or unique constraint violation.
    /// </summary>
    private const int UniqueViolation = 2601;

    /// <summary>
    /// SQL Server primary key violation.
    /// </summary>
    private const int DuplicateKey = 2627;

    /// <summary>
    /// SQL Server foreign key constraint violation.
    /// </summary>
    private const int ForeignKeyViolation = 547;

    /// <summary>
    /// Handles the exception and writes the problem response.
    /// </summary>
    /// <param name="httpContext">Current request.</param>
    /// <param name="exception">Thrown exception.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True when the exception was translated into a response.</returns>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Preserve security headers on error responses (MED-01)
        httpContext.Response.Headers["X-Content-Type-Options"] = "nosniff";
        httpContext.Response.Headers["X-Frame-Options"] = "DENY";
        httpContext.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        var (status, title) = Map(exception);

        // A client that disconnected mid-request is not a server fault, so it
        // is not logged as an error and there is nobody left to answer.
        if (httpContext.RequestAborted.IsCancellationRequested)
        {
            logger.LogInformation(
                "Request {Method} {Path} was aborted by the client.",
                httpContext.Request.Method,
                httpContext.Request.Path);

            return true;
        }

        if (status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Unhandled exception for {Method} {Path}.",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }
        else
        {
            logger.LogInformation(
                "Request {Method} {Path} rejected with {Status}: {Message}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                status,
                exception.Message);
        }

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = status == StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred. Use the traceId when reporting it."
                : exception.Message,
            Instance = httpContext.Request.Path
        };

        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        if (exception is ValidationException validationException)
        {
            problem.Extensions["errors"] = validationException.Errors
                .GroupBy(failure => failure.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(failure => failure.ErrorMessage).ToArray());
        }

        httpContext.Response.StatusCode = status;

        await httpContext.Response.WriteAsJsonAsync(
            problem,
            cancellationToken);

        return true;
    }

    /// <summary>
    /// Maps an exception to the status code and title of the response.
    /// </summary>
    /// <param name="exception">Thrown exception.</param>
    /// <returns>The status code and the title to report.</returns>
    private static (int Status, string Title) Map(Exception exception)
        => exception switch
        {
            ValidationException =>
                (StatusCodes.Status400BadRequest, "Validation failed."),

            ArgumentException =>
                (StatusCodes.Status400BadRequest, "The request is not valid."),

            InvalidDataException =>
                (StatusCodes.Status400BadRequest, "The submitted data is not valid."),

            KeyNotFoundException =>
                (StatusCodes.Status404NotFound, "The requested resource was not found."),

            UnauthorizedAccessException =>
                (StatusCodes.Status403Forbidden, "You are not allowed to perform this action."),

            InvalidOperationException =>
                (StatusCodes.Status409Conflict, "The request conflicts with the current state."),

            DbUpdateException databaseException
                when IsDuplicate(databaseException) =>
                (StatusCodes.Status409Conflict, "The record already exists."),

            DbUpdateException databaseException
                when IsForeignKey(databaseException) =>
                (StatusCodes.Status409Conflict, "The operation cannot be completed because related records exist."),

            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

    /// <summary>
    /// Detects a duplicate key violation reported by SQL Server.
    /// </summary>
    /// <param name="exception">Database exception.</param>
    /// <returns>True when the failure is a duplicate key or unique index.</returns>
    private static bool IsDuplicate(DbUpdateException exception)
        => exception.InnerException is SqlException sqlException &&
           (sqlException.Number == UniqueViolation ||
            sqlException.Number == DuplicateKey);

    /// <summary>
    /// Detects a foreign key constraint violation reported by SQL Server (QA-05).
    /// </summary>
    /// <param name="exception">Database exception.</param>
    /// <returns>True when the failure is a foreign key constraint violation.</returns>
    private static bool IsForeignKey(DbUpdateException exception)
        => exception.InnerException is SqlException sqlException &&
           sqlException.Number == ForeignKeyViolation;
}
