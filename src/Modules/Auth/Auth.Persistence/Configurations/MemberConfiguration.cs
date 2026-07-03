namespace Auth.Persistence.Configurations;

using Auth.Domain.Aggregates;
using Auth.Domain.Enums;
using Auth.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain;

internal sealed class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("members");
        builder.HasKey(member => member.Id);

        builder.Property(member => member.Id)
            .HasConversion(id => id.Value, value => new MemberId(value));

        builder.Property(member => member.TenantId)
            .HasMaxLength(TenantIds.MaxLength)
            .IsRequired();

        builder.Property(member => member.PasswordHash)
            .HasMaxLength(Member.PasswordHashMaxLength)
            .IsRequired();

        builder.Property(member => member.Status)
            .HasConversion<int>()
            .HasDefaultValue(MemberStatus.Active)
            .HasSentinel(default)
            .IsRequired();

        builder.Property(member => member.RegisteredAtUtc)
            .IsRequired();

        builder.Property(member => member.DisabledReason)
            .HasMaxLength(Member.DisabledReasonMaxLength);

        builder.HasIndex(member => new { member.TenantId, member.RegisteredAtUtc });

        builder.HasMany(member => member.Usernames)
            .WithOne()
            .HasForeignKey(username => username.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(member => member.Usernames)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(member => member.Sessions)
            .WithOne()
            .HasForeignKey(session => session.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(member => member.Sessions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
