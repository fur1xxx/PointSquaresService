using Microsoft.EntityFrameworkCore;
using Persistence.Core.Entities;

namespace Persistence.Core;

public class PointSquaresDbContext(DbContextOptions<PointSquaresDbContext> options) : DbContext(options)
{
    public DbSet<PointsCollectionEntity> PointsCollections => Set<PointsCollectionEntity>();
    public DbSet<PointEntity> Points => Set<PointEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PointSquaresDbContext).Assembly);
}
