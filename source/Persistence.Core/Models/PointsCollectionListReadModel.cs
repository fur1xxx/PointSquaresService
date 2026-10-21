namespace Persistence.Core.Models;

public record PointsCollectionListReadModel(IReadOnlyList<PointsCollectionReadModel> Items, bool Truncated);
