namespace Persistence.Core.Models;

public record PointsCollectionReadModel(long Id, string Name, int PointsCount, long SquaresCount, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
