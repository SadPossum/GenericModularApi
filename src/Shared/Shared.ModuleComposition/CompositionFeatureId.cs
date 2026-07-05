namespace Shared.ModuleComposition;

using Shared.Modules;

public readonly record struct CompositionFeatureId
{
    private readonly string? value;

    public CompositionFeatureId(string value)
        => this.value = ModuleMetadataNaming.NormalizeFeatureKey(value, nameof(value));

    public string Value => this.value ?? string.Empty;

    public override string ToString() => this.Value;
}
