namespace Auth.Persistence.Configurations;

using Shared.Naming;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Messaging.Infrastructure;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        builder.HasKey(message => message.Id);
        builder.Property(message => message.Subject).HasMaxLength(OutboxMessage.SubjectMaxLength).IsRequired();
        builder.Property(message => message.EventType).HasMaxLength(OutboxMessage.EventTypeMaxLength).IsRequired();
        builder.Property(message => message.TenantId).HasMaxLength(TenantIds.MaxLength).IsRequired();
        builder.Property(message => message.LockedBy).HasMaxLength(OutboxMessage.LockedByMaxLength);
        builder.Property(message => message.Payload).IsRequired();
        builder.HasIndex(message => new
        {
            message.ProcessedAtUtc,
            message.NextAttemptAtUtc,
            message.LockedUntilUtc,
            message.CreatedAtUtc
        });
    }
}
