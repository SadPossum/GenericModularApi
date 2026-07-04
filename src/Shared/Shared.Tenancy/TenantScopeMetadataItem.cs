namespace Shared.Tenancy;

using Shared.Modules;

public sealed record TenantScopeMetadataItem : ModuleMetadataItem
{
    public static readonly TenantScopeMetadataItem Instance = new();

    private TenantScopeMetadataItem()
        : base("tenancy.scope")
    {
    }
}
