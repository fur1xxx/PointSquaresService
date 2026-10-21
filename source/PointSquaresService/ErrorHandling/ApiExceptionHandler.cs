using System.Text.Json;
using Common.ErrorCodes;
using Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;

namespace PointSquaresService.ErrorHandling;

public partial class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    private static readonly ErrorResponse CollectionBusy = new(
        ResponseErrorCodes.CollectionBusy,
        "Another request is changing this collection. Try again."
    );

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, response) = exception switch
        {
            ValidationException e => (StatusCodes.Status400BadRequest, ErrorResponse.FromException(e)),
            NotFoundException e => (StatusCodes.Status404NotFound, ErrorResponse.FromException(e)),
            DomainException e => (StatusCodes.Status409Conflict, ErrorResponse.FromException(e)),
            BadHttpRequestException { InnerException: JsonException e } => (
                StatusCodes.Status400BadRequest,
                new ErrorResponse(ResponseErrorCodes.BadRequest, InvalidJsonMessage(e))
            ),
            BadHttpRequestException { StatusCode: StatusCodes.Status415UnsupportedMediaType } => (
                StatusCodes.Status415UnsupportedMediaType,
                new ErrorResponse(ResponseErrorCodes.BadRequest, "The request body must be JSON (Content-Type: application/json).")
            ),
            BadHttpRequestException e => (
                e.StatusCode,
                new ErrorResponse(ResponseErrorCodes.BadRequest, "The request body is missing or a query value cannot be read.")
            ),
            MySqlException e when IsLockConflict(e) => (StatusCodes.Status409Conflict, CollectionBusy),
            DbUpdateException { InnerException: MySqlException e } when IsLockConflict(e) => (StatusCodes.Status409Conflict, CollectionBusy),
            _ => (StatusCodes.Status500InternalServerError, new ErrorResponse(ResponseErrorCodes.InternalError, "An unexpected error occurred.")),
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
            LogUnhandledException(logger, exception, httpContext.Request.Method, httpContext.Request.Path);

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
        return true;
    }

    private static string InvalidJsonMessage(JsonException exception) =>
        $"The request body is not valid JSON or has a value of the wrong type at {exception.Path}, line {exception.LineNumber + 1}, position {exception.BytePositionInLine + 1}.";

    private static bool IsLockConflict(MySqlException exception) =>
        exception.Number is (int)MySqlErrorCode.LockWaitTimeout or (int)MySqlErrorCode.LockDeadlock;

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception on {Method} {Path}")]
    private static partial void LogUnhandledException(ILogger logger, Exception exception, string method, PathString path);
}
