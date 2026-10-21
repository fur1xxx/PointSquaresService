using System.Text.Json;
using Common.ErrorCodes;
using Common.Exceptions;
using PointSquaresService.Endpoints.Requests;
using Xunit;

namespace UnitTests;

public class PointRequest_Should
{
    [Fact]
    public async Task InvokeAsync_XMissing_ThrowsXIsRequired()
    {
        var exception = await Assert.ThrowsAsync<ValidationException>(() => RequestValidation.ValidateAsync(new PointRequest { Y = 1 }));

        Assert.Equal("X is required.", exception.Message);
        Assert.Equal(ResponseErrorCodes.BadRequest, exception.ErrorCode);
    }

    [Fact]
    public async Task InvokeAsync_YMissing_ThrowsYIsRequired()
    {
        var exception = await Assert.ThrowsAsync<ValidationException>(() => RequestValidation.ValidateAsync(new PointRequest { X = 1 }));

        Assert.Equal("Y is required.", exception.Message);
        Assert.Equal(ResponseErrorCodes.BadRequest, exception.ErrorCode);
    }

    [Fact]
    public async Task InvokeAsync_XAndYAtIntRangeEnds_CallsNext()
    {
        var nextCalled = await RequestValidation.ValidateAsync(new PointRequest { X = int.MinValue, Y = int.MaxValue });

        Assert.True(nextCalled);
    }

    [Fact]
    public void Deserialize_XAsString_Throws()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<PointRequest>("""{"x":"5","y":1}""", JsonSerializerOptions.Web));
    }
}
