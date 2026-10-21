using System.ComponentModel.DataAnnotations;

namespace PointSquaresService.Endpoints.Requests;

public record CreatePointsCollectionRequest
{
    [Required(ErrorMessage = "{0} is required.")]
    [MaxLength(200, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string? Name { get; init; }
}
