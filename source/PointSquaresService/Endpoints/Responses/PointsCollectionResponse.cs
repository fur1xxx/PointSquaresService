using Persistence.Core.Models;

namespace PointSquaresService.Endpoints.Responses;

public record PointsCollectionResponse(long Id, string Name, int PointsCount, long SquaresCount, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt)
{
    public static PointsCollectionResponse From(PointsCollectionReadModel model) =>
        new(model.Id, model.Name, model.PointsCount, model.SquaresCount, model.CreatedAt, model.UpdatedAt);
}
