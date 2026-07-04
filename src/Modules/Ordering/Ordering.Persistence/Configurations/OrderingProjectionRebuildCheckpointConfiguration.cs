namespace Ordering.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Naming;

internal sealed class OrderingProjectionRebuildCheckpointConfiguration
    : IEntityTypeConfiguration<OrderingProjectionRebuildCheckpoint>
{
    public void Configure(EntityTypeBuilder<OrderingProjectionRebuildCheckpoint> builder)
    {
        builder.ToTable("projection_rebuild_checkpoints");
        builder.HasKey(checkpoint => new
        {
            checkpoint.TenantId,
            checkpoint.ProjectionName,
            checkpoint.RunId
        });
        builder.Property(checkpoint => checkpoint.TenantId).HasMaxLength(TenantIds.MaxLength).IsRequired();
        builder.Property(checkpoint => checkpoint.ProjectionName)
            .HasMaxLength(OrderingProjectionRebuildCheckpoint.ProjectionNameMaxLength)
            .IsRequired();
        builder.Property(checkpoint => checkpoint.Cursor)
            .HasMaxLength(OrderingProjectionRebuildCheckpoint.CursorMaxLength);
        builder.Property(checkpoint => checkpoint.ProjectionVersion).IsRequired();
        builder.Property(checkpoint => checkpoint.ProcessedCount).IsRequired();
        builder.Property(checkpoint => checkpoint.WrittenCount).IsRequired();
        builder.Property(checkpoint => checkpoint.SkippedCount).IsRequired();
        builder.Property(checkpoint => checkpoint.FailedCount).IsRequired();
        builder.Property(checkpoint => checkpoint.UpdatedAtUtc).IsRequired();
        builder.Property(checkpoint => checkpoint.CompletedAtUtc);
    }
}
