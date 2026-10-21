using Microsoft.AspNetCore.Http;
using PointSquaresService.Validators;

namespace UnitTests;

public static class RequestValidation
{
    public static async Task<bool> ValidateAsync<TRequest>(TRequest request)
        where TRequest : notnull
    {
        var nextCalled = false;
        var context = new DefaultEndpointFilterInvocationContext(new DefaultHttpContext(), request);

        await new ValidateModel<TRequest>().InvokeAsync(
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
