using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Core.Entities;

namespace Persistence.Core.Configurations;

public class PointsCollectionEntityConfiguration : IEntityTypeConfiguration<PointsCollectionEntity>
{
    public void Configure(EntityTypeBuilder<PointsCollectionEntity> builder)
    {
        builder.ToTable("points_collections");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.Name).HasColumnName("name").HasColumnType("varchar(200)");
        builder.Property(c => c.PointsCount).HasColumnName("points_count");
        builder.Property(c => c.SquaresCount).HasColumnName("squares_count");
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").HasColumnType("datetime(6)").HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime(6)").HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
    }
}
