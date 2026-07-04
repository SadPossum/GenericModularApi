namespace Auth.Persistence.Configurations;

using Shared.Naming;
using Auth.Domain.Entities;
using Auth.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class MemberUsernameConfiguration : IEntityTypeConfiguration<MemberUsername>
{
    public void Configure(EntityTypeBuilder<MemberUsername> builder)
    {
        builder.ToTable("member_usernames");
        builder.HasKey(username => username.Id);

        builder.Property(username => username.Id)
            .HasConversion(id => id.Value, value => new MemberUsernameId(value));

        builder.Property(username => username.MemberId)
            .HasConversion(id => id.Value, value => new MemberId(value));

        builder.Property(username => username.TenantId)
            .HasMaxLength(TenantIds.MaxLength)
            .IsRequired();

        builder.Property(username => username.Value)
            .HasMaxLength(MemberUsername.ValueMaxLength)
            .IsRequired();

        builder.Property(username => username.NormalizedValue)
            .HasMaxLength(MemberUsername.NormalizedValueMaxLength)
            .IsRequired();

        builder.HasIndex(username => new { username.TenantId, username.NormalizedValue })
            .IsUnique();
    }
}
