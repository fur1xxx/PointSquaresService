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

    public async Task<bool> LockAsync(long id, CancellationToken cancellationToken)
    {
        if (context.Database.CurrentTransaction is null)
            throw new InvalidOperationException("LockAsync must run inside a transaction, or MySQL releases the lock at once.");

        await context.Database.ExecuteSqlAsync($"SET SESSION innodb_lock_wait_timeout = 10", cancellationToken);
        var ids = await context
            .Database.SqlQuery<long>($"SELECT id AS Value FROM points_collections WHERE id = {id} FOR UPDATE")
            .ToListAsync(cancellationToken);
        return ids.Count > 0;
    }

    public Task ChangeCountsAsync(long id, int pointsChange, long squaresChange, CancellationToken cancellationToken) =>
        context.Database.ExecuteSqlAsync(
            $"""
            UPDATE points_collections
            SET points_count = points_count + {pointsChange},
                squares_count = squares_count + {squaresChange},
                updated_at = CURRENT_TIMESTAMP(6)
            WHERE id = {id}
            """,
            cancellationToken
        );
}
