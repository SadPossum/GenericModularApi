namespace Auth.Persistence.Configurations;

using Auth.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).HasConversion(
            memberId => memberId.Value,
            value => new(value));

        builder.Metadata.FindNavigation(nameof(Member.Usernames))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(a => a.Usernames)
            .WithOne();

        builder.Metadata.FindNavigation(nameof(Member.Sessions))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(a => a.Sessions)
            .WithOne();

    }
}
