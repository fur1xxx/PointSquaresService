using Common.ErrorCodes;
using Common.Exceptions;
using Microsoft.AspNetCore.Http;
using PointSquaresService.Endpoints.Requests;
using PointSquaresService.Validators;
using Xunit;

namespace UnitTests;

public class GetPointsCollectionsRequest_Should
{
    [Theory]
    [InlineData(0)]
    [InlineData(10_001)]
    public async Task InvokeAsync_LimitOutOfRange_ThrowsLimitMustBeBetween1And10000(int limit)
    {
        var exception = await Assert.ThrowsAsync<ValidationException>(() => ValidateAsync(new GetPointsCollectionsRequest(limit)));

        Assert.Equal("Limit must be between 1 and 10000.", exception.Message);
        Assert.Equal(ResponseErrorCodes.BadRequest, exception.ErrorCode);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10_000)]
    public async Task InvokeAsync_LimitInRange_CallsNext(int limit)
    {
        var nextCalled = await ValidateAsync(new GetPointsCollectionsRequest(limit));

        Assert.True(nextCalled);
    }

    [Fact]
    public void Limit_NotSet_Is1000()
    {
        Assert.Equal(1_000, new GetPointsCollectionsRequest().Limit);
    }

    private static async Task<bool> ValidateAsync(GetPointsCollectionsRequest request)
    {
        var nextCalled = false;
        var context = new DefaultEndpointFilterInvocationContext(new DefaultHttpContext(), request);

        await new ValidateModel<GetPointsCollectionsRequest>().InvokeAsync(
            context,
            _ =>
            {
                nextCalled = true;
                return ValueTask.FromResult<object?>(null);
            }
        );

        return nextCalled;
    }
}
