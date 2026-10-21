using Common.Exceptions;

namespace PointSquaresService.ErrorHandling;

public record ErrorResponse(string Error, string Message)
{
    public static ErrorResponse FromException(ApiException exception) => new(exception.ErrorCode, exception.Message);
}
