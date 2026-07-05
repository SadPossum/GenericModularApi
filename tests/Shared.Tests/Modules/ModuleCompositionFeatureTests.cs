namespace Shared.Tests;

using Microsoft.Extensions.Hosting;
using Shared.ModuleComposition;
using Shared.Modules;
using Xunit;

[Trait("Category", "Unit")]
public sealed class ModuleCompositionFeatureTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("feature")]
    [InlineData("feature.")]
    [InlineData(".feature")]
    [InlineData("feature..one")]
    [InlineData("feature_one.enabled")]
    public void Feature_ids_reject_invalid_values(string? value)
    {
        Assert.ThrowsAny<ArgumentException>(() => new CompositionFeatureId(value!));
    }

    [Fact]
    public void Feature_ids_normalize_stable_dotted_kebab_names()
    {
        CompositionFeatureId id = new(" Messaging.Outbox ");

        Assert.Equal("messaging.outbox", id.Value);
        Assert.Equal("messaging.outbox", id.ToString());
    }

    [Fact]
    public void Missing_required_feature_fails_with_clear_error()
    {
        ModuleProfileDescriptor profile = new(
            "ordering",
            "live-projections",
            requires:
            [
                new RequiredCompositionFeature(
                    new CompositionFeatureId("messaging.nats-consumers"),
                    "ordering/live-projections",
                    reason: "Enable the NATS consumer adapter.")
            ]);

        ModuleCompositionValidationResult result = ModuleCompositionValidator.Validate(new ModuleCompositionSnapshot(
            selectedProfiles: [new SelectedModuleProfile(profile)]));

        Assert.False(result.IsValid);
        string error = Assert.Single(result.Errors);
        Assert.Contains("ordering/live-projections", error, StringComparison.Ordinal);
        Assert.Contains("messaging.nats-consumers", error, StringComparison.Ordinal);
        Assert.Contains("Enable the NATS consumer adapter.", error, StringComparison.Ordinal);
    }

    [Fact]
    public void Optional_missing_feature_is_reported_but_does_not_fail()
    {
        ModuleProfileDescriptor profile = new(
            "catalog",
            "default",
            requires:
            [
                new RequiredCompositionFeature(
                    new CompositionFeatureId("caching.application"),
                    "catalog/default",
                    optional: true,
                    reason: "Catalog can fall back to direct reads.")
            ]);

        ModuleCompositionValidationResult result = ModuleCompositionValidator.Validate(new ModuleCompositionSnapshot(
            selectedProfiles: [new SelectedModuleProfile(profile)]));

        Assert.True(result.IsValid);
        Assert.Contains("optional missing", result.Report, StringComparison.Ordinal);
    }

    [Fact]
    public void Missing_required_module_fails()
    {
        ModuleProfileDescriptor profile = new(
            "ordering",
            "default",
            requiredModules:
            [
                new RequiredCompositionModule(
                    "catalog",
                    "ordering/default",
                    reason: "Ordering example needs Catalog contracts and projections.")
            ]);

        ModuleCompositionValidationResult result = ModuleCompositionValidator.Validate(new ModuleCompositionSnapshot(
            selectedProfiles: [new SelectedModuleProfile(profile)]));

        Assert.False(result.IsValid);
        Assert.Contains("requires module 'catalog'", Assert.Single(result.Errors), StringComparison.Ordinal);
    }

    [Fact]
    public void Duplicate_exclusive_feature_providers_fail_deterministically()
    {
        CompositionFeatureId feature = new("messaging.outbox");

        ModuleCompositionValidationResult result = ModuleCompositionValidator.Validate(new ModuleCompositionSnapshot(
            providedFeatures:
            [
                new ProvidedCompositionFeature(feature, "adapter-b"),
                new ProvidedCompositionFeature(feature, "adapter-a")
            ]));

        Assert.False(result.IsValid);
        Assert.Equal(
            "Feature 'messaging.outbox' is provided by multiple providers ('adapter-a', 'adapter-b') but is exclusive.",
            Assert.Single(result.Errors));
    }

    [Fact]
    public void Duplicate_multi_provider_features_are_allowed()
    {
        CompositionFeatureId feature = new("tenancy.context");

        ModuleCompositionValidationResult result = ModuleCompositionValidator.Validate(new ModuleCompositionSnapshot(
            providedFeatures:
            [
                new ProvidedCompositionFeature(feature, "Shared.Tenancy.Infrastructure", allowMultipleProviders: true),
                new ProvidedCompositionFeature(feature, "tenancy/default", allowMultipleProviders: true)
            ]));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Multiple_profiles_for_same_module_fail()
    {
        ModuleProfileDescriptor global = new("auth", "global");
        ModuleProfileDescriptor tenantScoped = new("auth", "tenant-scoped");

        ModuleCompositionValidationResult result = ModuleCompositionValidator.Validate(new ModuleCompositionSnapshot(
            selectedProfiles: [new SelectedModuleProfile(global), new SelectedModuleProfile(tenantScoped)]));

        Assert.False(result.IsValid);
        Assert.Contains("Module 'auth' has multiple selected profiles", Assert.Single(result.Errors), StringComparison.Ordinal);
    }

    [Fact]
    public void Descriptor_metadata_exposes_composition_profiles()
    {
        ModuleProfileDescriptor profile = new(
            "catalog",
            "default",
            provides: [new ProvidedCompositionFeature(new CompositionFeatureId("catalog.items"), "catalog/default")]);

        ModuleDescriptor descriptor = ModuleDescriptor
            .Create("catalog")
            .WithProfile(profile)
            .Build();

        ModuleProfileDescriptor actual = Assert.Single(descriptor.GetCompositionProfiles());
        Assert.Equal("catalog", actual.ModuleName);
        Assert.Equal("default", actual.ProfileName);
        Assert.Equal("catalog.items", Assert.Single(actual.Provides).Id.Value);
    }

    [Fact]
    public void Builder_validation_fails_without_starting_host()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        builder.SelectModuleProfile(new ModuleProfileDescriptor(
            "auth",
            "tenant-scoped",
            requires: [new RequiredCompositionFeature(new CompositionFeatureId("tenancy.context"), "auth/tenant-scoped")]));

        ModuleCompositionValidationException exception = Assert.Throws<ModuleCompositionValidationException>(
            () => builder.ValidateModuleComposition());

        Assert.Contains("tenancy.context", exception.Message, StringComparison.Ordinal);
    }
}
