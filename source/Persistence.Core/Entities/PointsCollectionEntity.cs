namespace Persistence.Core.Entities;

public class PointsCollectionEntity
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public int PointsCount { get; set; }
    public long SquaresCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
