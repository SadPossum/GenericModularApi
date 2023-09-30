namespace Auth.Persistence.Configurations;

using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal class MemberSessionConfiguration : IEntityTypeConfiguration<MemberSession>
{
    public void Configure(EntityTypeBuilder<MemberSession> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).HasConversion(
            memberSessionId => memberSessionId.Value,
            value => new(value));

        builder.ComplexProperty(a => a.AccessToken);
        builder.ComplexProperty(a => a.RefreshToken);
    }
}
