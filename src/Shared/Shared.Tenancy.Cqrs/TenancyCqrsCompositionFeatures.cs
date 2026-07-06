namespace Shared.Tenancy.Cqrs;

using Shared.ModuleComposition;

public static class TenancyCqrsCompositionFeatures
{
    public static readonly CompositionFeatureId LogScope = new("tenancy.cqrs-log-scope");

    public static ProvidedCompositionFeature LogScopeProvided(string provider) =>
        new(LogScope, provider, "Tenant context contributes bounded CQRS logging scope metadata.");

    public static RequiredCompositionFeature LogScopeRequired(string owner, string? reason = null, bool optional = false) =>
        new(LogScope, owner, optional, reason);
}
