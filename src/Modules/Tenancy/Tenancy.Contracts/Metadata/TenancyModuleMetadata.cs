namespace Tenancy.Contracts;

using Shared.Modules;

public static class TenancyModuleMetadata
{
    public const string Name = "tenancy";

    public static ModuleDescriptor Descriptor { get; } = ModuleDescriptor
        .Create(Name)
        .Build();
}
