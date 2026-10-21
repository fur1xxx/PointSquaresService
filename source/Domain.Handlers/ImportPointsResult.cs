using Persistence.Core.Models;

namespace Domain.Handlers;

public record ImportPointsResult(int Added, int AlreadyInCollection, PointsCollectionReadModel Collection);
