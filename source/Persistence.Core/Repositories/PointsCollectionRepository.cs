using Persistence.Core.Models;

namespace Persistence.Core.Repositories;

public class PointsCollectionRepository(PointSquaresDbContext context)
{
    private readonly PointSquaresDbContext context = context;

    public Task<PointsCollectionReadModel?> GetAsync(long id, CancellationToken cancellationToken) => throw new NotImplementedException();
}
