namespace Catalog.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain;
using Shared.Infrastructure.Messaging;

internal sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("inbox_messages");
        builder.HasKey(message => new { message.Id, message.Handler });
        builder.Property(message => message.Handler).HasMaxLength(InboxMessage.HandlerMaxLength).IsRequired();
        builder.Property(message => message.Subject).HasMaxLength(InboxMessage.SubjectMaxLength).IsRequired();
        builder.Property(message => message.EventType).HasMaxLength(InboxMessage.EventTypeMaxLength).IsRequired();
        builder.Property(message => message.TenantId).HasMaxLength(TenantIds.MaxLength).IsRequired();
        builder.Property(message => message.Status).HasConversion<int>().IsRequired();
        builder.Property(message => message.LockedBy).HasMaxLength(InboxMessage.LockedByMaxLength);
        builder.Property(message => message.LastError).HasMaxLength(InboxMessage.LastErrorMaxLength);
        builder.HasIndex(message => new { message.Handler, message.Status });
    }
}
