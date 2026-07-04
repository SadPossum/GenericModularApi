namespace Shared.Api.Observability;

using Shared.Naming;

public sealed record ModuleEndpointMetadata
{
    public ModuleEndpointMetadata(string moduleName) =>
        this.ModuleName = SharedModuleNames.Normalize(moduleName);

    public string ModuleName { get; }
}
