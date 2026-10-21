using System.ComponentModel.DataAnnotations;

namespace PointSquaresService.Endpoints.Requests;

public record GetPointsCollectionsRequest([property: Range(1, 10_000, ErrorMessage = "{0} must be between {1} and {2}.")] int Limit = 1_000);
