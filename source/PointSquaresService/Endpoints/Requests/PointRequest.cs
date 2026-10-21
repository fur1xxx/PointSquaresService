using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PointSquaresService.Endpoints.Requests;

[JsonNumberHandling(JsonNumberHandling.Strict)]
public record PointRequest
{
    [Required(ErrorMessage = "{0} is required.")]
    public int? X { get; init; }

    [Required(ErrorMessage = "{0} is required.")]
    public int? Y { get; init; }
}
