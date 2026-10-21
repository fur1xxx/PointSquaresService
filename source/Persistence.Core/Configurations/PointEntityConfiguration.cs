using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Core.Entities;

namespace Persistence.Core.Configurations;

public class PointEntityConfiguration : IEntityTypeConfiguration<PointEntity>
{
    public void Configure(EntityTypeBuilder<PointEntity> builder)
    {
        builder.ToTable("points");
        builder.HasKey(p => new
        {
            p.CollectionId,
            p.X,
            p.Y,
        });
        builder.Property(p => p.CollectionId).HasColumnName("collection_id");
        builder.Property(p => p.X).HasColumnName("x");
        builder.Property(p => p.Y).HasColumnName("y");
        builder
            .HasIndex(p => new
            {
                p.CollectionId,
                p.Y,
                p.X,
            })
            .HasDatabaseName("ix_points_collection_id_y_x");
        builder.HasOne<PointsCollectionEntity>().WithMany().HasForeignKey(p => p.CollectionId).OnDelete(DeleteBehavior.Restrict);
    }
}
