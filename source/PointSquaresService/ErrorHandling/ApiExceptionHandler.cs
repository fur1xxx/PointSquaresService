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
            BadHttpRequestException e => (e.StatusCode, new ErrorResponse(ResponseErrorCodes.BadRequest, e.Message)),
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

    private static bool IsLockConflict(MySqlException exception) =>
        exception.Number is (int)MySqlErrorCode.LockWaitTimeout or (int)MySqlErrorCode.LockDeadlock;

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception on {Method} {Path}")]
    private static partial void LogUnhandledException(ILogger logger, Exception exception, string method, PathString path);
}
