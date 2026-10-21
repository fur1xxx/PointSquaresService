using System.ComponentModel.DataAnnotations;
using Common.ErrorCodes;
using ValidationException = Common.Exceptions.ValidationException;

namespace PointSquaresService.Validators;

public class ValidateModel<TRequest> : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<TRequest>().First()!;

        var validationResults = new List<ValidationResult>();
        var validationSuccessful = Validator.TryValidateObject(request, new ValidationContext(request), validationResults, true);

        if (validationSuccessful)
            return await next(context);

        var errorMessage = validationResults.FirstOrDefault()?.ErrorMessage ?? "Validation failed.";
        throw new ValidationException(errorMessage, ResponseErrorCodes.BadRequest);
    }
}
