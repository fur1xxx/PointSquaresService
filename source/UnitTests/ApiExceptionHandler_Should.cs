using Common.ErrorCodes;
using Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using PointSquaresService.ErrorHandling;
using Xunit;

namespace UnitTests;

public class ApiExceptionHandler_Should
{
    private readonly ApiExceptionHandler handler = new(NullLogger<ApiExceptionHandler>.Instance);

    [Fact]
    public async Task TryHandleAsync_ValidationException_Returns400WithCodeAndMessage()
    {
        var (statusCode, body) = await HandleAsync(new ValidationException("X is required.", ResponseErrorCodes.BadRequest));

        Assert.Equal(StatusCodes.Status400BadRequest, statusCode);
        Assert.Equal("""{"error":"BAD_REQUEST","message":"X is required."}""", body);
    }

    [Fact]
    public async Task TryHandleAsync_NotFoundException_Returns404WithCodeAndMessage()
    {
        var (statusCode, body) = await HandleAsync(new NotFoundException("Collection 7 does not exist.", "COLLECTION_NOT_FOUND"));

        Assert.Equal(StatusCodes.Status404NotFound, statusCode);
        Assert.Equal("""{"error":"COLLECTION_NOT_FOUND","message":"Collection 7 does not exist."}""", body);
    }

    [Fact]
    public async Task TryHandleAsync_DomainException_Returns409WithCodeAndMessage()
    {
        var (statusCode, body) = await HandleAsync(new DomainException("Point (1, 2) is already in collection 7.", "POINT_ALREADY_EXISTS"));

        Assert.Equal(StatusCodes.Status409Conflict, statusCode);
        Assert.Equal("""{"error":"POINT_ALREADY_EXISTS","message":"Point (1, 2) is already in collection 7."}""", body);
    }

    [Fact]
    public async Task TryHandleAsync_BadHttpRequestException_ReturnsItsStatusWithBadRequestCode()
    {
        var (statusCode, body) = await HandleAsync(new BadHttpRequestException("Request body is not valid JSON.", StatusCodes.Status400BadRequest));

        Assert.Equal(StatusCodes.Status400BadRequest, statusCode);
        Assert.Equal("""{"error":"BAD_REQUEST","message":"Request body is not valid JSON."}""", body);
    }

    [Fact]
    public async Task TryHandleAsync_UnknownException_Returns500WithoutExceptionDetails()
    {
        var (statusCode, body) = await HandleAsync(new InvalidOperationException("Server=db;Password=secret"));

        Assert.Equal(StatusCodes.Status500InternalServerError, statusCode);
        Assert.Equal("""{"error":"INTERNAL_ERROR","message":"An unexpected error occurred."}""", body);
    }

    private async Task<(int StatusCode, string Body)> HandleAsync(Exception exception)
    {
        var httpContext = new DefaultHttpContext();
        using var responseBody = new MemoryStream();
        httpContext.Response.Body = responseBody;

        var handled = await handler.TryHandleAsync(httpContext, exception, TestContext.Current.CancellationToken);

        Assert.True(handled);
        responseBody.Position = 0;
        using var reader = new StreamReader(responseBody);
        return (httpContext.Response.StatusCode, await reader.ReadToEndAsync(TestContext.Current.CancellationToken));
    }
}
