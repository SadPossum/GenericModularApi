namespace Administration.Persistence.Configurations;

using Administration.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Administration;

internal sealed class AdminRolePermissionConfiguration : IEntityTypeConfiguration<AdminRolePermission>
{
    public void Configure(EntityTypeBuilder<AdminRolePermission> builder)
    {
        builder.ToTable("role_permissions");
        builder.HasKey(permission => permission.Id);

        builder.Property(permission => permission.PermissionCode).HasMaxLength(AdminPermission.MaxLength).IsRequired();
        builder.HasIndex(permission => new { permission.RoleId, permission.PermissionCode }).IsUnique();
    }
}
