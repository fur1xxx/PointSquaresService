using Persistence.Core.Models;

namespace PointSquaresService.Endpoints.Responses;

public record GetPointsCollectionsResponse(IReadOnlyList<PointsCollectionResponse> Items, bool Truncated)
{
    public static GetPointsCollectionsResponse From(PointsCollectionListReadModel model) =>
        new(model.Items.Select(PointsCollectionResponse.From).ToList(), model.Truncated);
}
