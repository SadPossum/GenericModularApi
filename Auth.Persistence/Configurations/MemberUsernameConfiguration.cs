namespace Auth.Persistence.Configurations;

using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal class MemberUsernameConfiguration : IEntityTypeConfiguration<MemberUsername>
{
    public void Configure(EntityTypeBuilder<MemberUsername> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).HasConversion(
            memberUsernameId => memberUsernameId.Value,
            value => new(value));

        builder.HasIndex(a => a.Value).IsUnique();
    }
}
