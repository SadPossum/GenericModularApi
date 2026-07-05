namespace Tenancy.Contracts;

using Shared.ModuleComposition;
using Shared.Modules;

public static class TenancyModuleMetadata
{
    public const string Name = "tenancy";

    public static ModuleDescriptor Descriptor { get; } = ModuleDescriptor
        .Create(Name)
        .WithProfile(TenancyProfiles.Default)
        .Build();
}
