using Common.ErrorCodes;
using Common.Exceptions;
using Microsoft.AspNetCore.Http;
using PointSquaresService.Endpoints.Requests;
using PointSquaresService.Validators;
using Xunit;

namespace UnitTests;

public class CreatePointsCollectionRequest_Should
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task InvokeAsync_NameMissingOrBlank_ThrowsNameIsRequired(string? name)
    {
        var exception = await Assert.ThrowsAsync<ValidationException>(() => ValidateAsync(new CreatePointsCollectionRequest { Name = name }));

        Assert.Equal("Name is required.", exception.Message);
        Assert.Equal(ResponseErrorCodes.BadRequest, exception.ErrorCode);
    }

    [Fact]
    public async Task InvokeAsync_NameOver200Characters_ThrowsNameCannotExceed200Characters()
    {
        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            ValidateAsync(new CreatePointsCollectionRequest { Name = new string('a', 201) })
        );

        Assert.Equal("Name cannot exceed 200 characters.", exception.Message);
        Assert.Equal(ResponseErrorCodes.BadRequest, exception.ErrorCode);
    }

    [Fact]
    public async Task InvokeAsync_NameOf200Characters_CallsNext()
    {
        var nextCalled = await ValidateAsync(new CreatePointsCollectionRequest { Name = new string('a', 200) });

        Assert.True(nextCalled);
    }

    private static async Task<bool> ValidateAsync(CreatePointsCollectionRequest request)
    {
        var nextCalled = false;
        var context = new DefaultEndpointFilterInvocationContext(new DefaultHttpContext(), request);

        await new ValidateModel<CreatePointsCollectionRequest>().InvokeAsync(
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
