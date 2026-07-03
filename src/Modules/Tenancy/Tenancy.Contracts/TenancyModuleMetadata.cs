namespace Tenancy.Contracts;

using Shared.Application.Modules;

public static class TenancyModuleMetadata
{
    public const string Name = "tenancy";

    public static ModuleDescriptor Descriptor { get; } = ModuleDescriptor.Empty(Name);
}
