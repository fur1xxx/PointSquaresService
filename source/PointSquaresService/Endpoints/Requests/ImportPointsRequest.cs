using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace PointSquaresService.Endpoints.Requests;

public record ImportPointsRequest : IValidatableObject
{
    public const int MaxRows = 100_000;

    public JsonElement Points { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Points.ValueKind != JsonValueKind.Array || Points.GetArrayLength() is 0 or > MaxRows)
            yield return new ValidationResult($"Points must be an array of 1 to {MaxRows} rows.");
    }
}
