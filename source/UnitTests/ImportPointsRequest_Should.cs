using System.Text.Json;
using Common.ErrorCodes;
using Common.Exceptions;
using PointSquaresService.Endpoints.Requests;
using Xunit;

namespace UnitTests;

public class ImportPointsRequest_Should
{
    [Theory]
    [InlineData("{}")]
    [InlineData("""{"points":null}""")]
    [InlineData("""{"points":5}""")]
    [InlineData("""{"points":{}}""")]
    [InlineData("""{"points":[]}""")]
    public async Task InvokeAsync_PointsMissingNullNotArrayOrEmpty_Throws(string json)
    {
        var exception = await Assert.ThrowsAsync<ValidationException>(() => RequestValidation.ValidateAsync(Deserialize(json)));

        Assert.Equal("Points must be an array of 1 to 100000 rows.", exception.Message);
        Assert.Equal(ResponseErrorCodes.BadRequest, exception.ErrorCode);
    }

    [Fact]
    public async Task InvokeAsync_100000Rows_CallsNext()
    {
        var nextCalled = await RequestValidation.ValidateAsync(Deserialize(WithRows(100_000)));

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_100001Rows_Throws()
    {
        var exception = await Assert.ThrowsAsync<ValidationException>(() => RequestValidation.ValidateAsync(Deserialize(WithRows(100_001))));

        Assert.Equal("Points must be an array of 1 to 100000 rows.", exception.Message);
    }

    private static ImportPointsRequest Deserialize(string json) => JsonSerializer.Deserialize<ImportPointsRequest>(json, JsonSerializerOptions.Web)!;

    private static string WithRows(int count) => $$"""{"points":[{{string.Join(",", Enumerable.Repeat("0", count))}}]}""";
}
