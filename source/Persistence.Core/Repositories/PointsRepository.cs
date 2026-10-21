using Domain.Core;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Persistence.Core.Entities;

namespace Persistence.Core.Repositories;

public class PointsRepository(PointSquaresDbContext context)
{
    public Task<bool> ExistsAsync(long collectionId, Point point, CancellationToken cancellationToken) =>
        context.Points.AnyAsync(p => p.CollectionId == collectionId && p.X == point.X && p.Y == point.Y, cancellationToken);

    public async Task AddAsync(long collectionId, Point point, CancellationToken cancellationToken)
    {
        context.Points.Add(
            new PointEntity
            {
                CollectionId = collectionId,
                X = point.X,
                Y = point.Y,
            }
        );
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRangeAsync(long collectionId, IEnumerable<Point> points, CancellationToken cancellationToken)
    {
        context.Points.AddRange(
            points.Select(point => new PointEntity
            {
                CollectionId = collectionId,
                X = point.X,
                Y = point.Y,
            })
        );
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> RemoveAsync(long collectionId, Point point, CancellationToken cancellationToken) =>
        await context.Points.Where(p => p.CollectionId == collectionId && p.X == point.X && p.Y == point.Y).ExecuteDeleteAsync(cancellationToken) > 0;

    public async Task<List<Point>> GetColumnAndRowAsync(long collectionId, Point point, CancellationToken cancellationToken)
    {
        var column = await context
            .Points.Where(p => p.CollectionId == collectionId && p.X == point.X)
            .Select(p => new Point(p.X, p.Y))
            .ToListAsync(cancellationToken);
        var row = await context
            .Points.Where(p => p.CollectionId == collectionId && p.Y == point.Y)
            .Select(p => new Point(p.X, p.Y))
            .ToListAsync(cancellationToken);
        return [.. column, .. row];
    }

    public Task<List<Point>> GetAllAsync(long collectionId, CancellationToken cancellationToken) =>
        context.Points.Where(p => p.CollectionId == collectionId).Select(p => new Point(p.X, p.Y)).ToListAsync(cancellationToken);

    public async Task<long> CountExistingAsync(long collectionId, IReadOnlyList<Point> points, CancellationToken cancellationToken)
    {
        var count = 0L;
        foreach (var chunk in points.Chunk(1_000))
        {
            var pairs = string.Join(", ", chunk.Select((_, i) => $"(@x{i}, @y{i})"));
            var sql = $"SELECT COUNT(*) AS Value FROM points WHERE collection_id = @collectionId AND (x, y) IN ({pairs})";
            var parameters = chunk
                .SelectMany((point, i) => new[] { new MySqlParameter($"@x{i}", point.X), new MySqlParameter($"@y{i}", point.Y) })
                .Append(new MySqlParameter("@collectionId", collectionId));
            count += await context.Database.SqlQueryRaw<long>(sql, [.. parameters]).SingleAsync(cancellationToken);
        }
        return count;
    }
}
