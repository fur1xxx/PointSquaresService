using Microsoft.EntityFrameworkCore;
using Persistence.Core.Entities;
using Persistence.Core.Models;

namespace Persistence.Core.Repositories;

public class PointsCollectionRepository(PointSquaresDbContext context)
{
    public async Task<PointsCollectionReadModel> CreateAsync(string name, CancellationToken cancellationToken)
    {
        var entity = new PointsCollectionEntity { Name = name };
        context.PointsCollections.Add(entity);
        await context.SaveChangesAsync(cancellationToken);
        return new PointsCollectionReadModel(
            entity.Id,
            entity.Name,
            entity.PointsCount,
            entity.SquaresCount,
            new DateTimeOffset(entity.CreatedAt, TimeSpan.Zero),
            new DateTimeOffset(entity.UpdatedAt, TimeSpan.Zero)
        );
    }

    public async Task<PointsCollectionListReadModel> ListAsync(int limit, CancellationToken cancellationToken)
    {
        var rows = await context
            .PointsCollections.OrderByDescending(c => c.Id)
            .Take(limit + 1)
            .Select(c => new PointsCollectionReadModel(
                c.Id,
                c.Name,
                c.PointsCount,
                c.SquaresCount,
                new DateTimeOffset(c.CreatedAt, TimeSpan.Zero),
                new DateTimeOffset(c.UpdatedAt, TimeSpan.Zero)
            ))
            .ToListAsync(cancellationToken);
        return new PointsCollectionListReadModel(rows.Take(limit).ToList(), rows.Count > limit);
    }

    public Task<PointsCollectionReadModel?> GetAsync(long id, CancellationToken cancellationToken) =>
        context
            .PointsCollections.Where(c => c.Id == id)
            .Select(c => new PointsCollectionReadModel(
                c.Id,
                c.Name,
                c.PointsCount,
                c.SquaresCount,
                new DateTimeOffset(c.CreatedAt, TimeSpan.Zero),
                new DateTimeOffset(c.UpdatedAt, TimeSpan.Zero)
            ))
            .SingleOrDefaultAsync(cancellationToken);
}
