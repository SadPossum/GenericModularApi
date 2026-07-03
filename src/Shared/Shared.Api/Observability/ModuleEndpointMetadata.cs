namespace Shared.Api.Observability;

using Shared.Application.Messaging;

public sealed record ModuleEndpointMetadata
{
    public ModuleEndpointMetadata(string moduleName) =>
        this.ModuleName = IntegrationEventNaming.NormalizeModuleName(moduleName);

    public string ModuleName { get; }
}
