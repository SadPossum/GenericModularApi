namespace Auth.Tests.Contracts;

using Auth.Contracts;
using Shared.ModuleComposition;
using Shared.Tenancy;
using Tenancy.Contracts;
using Xunit;

[Trait("Category", "Unit")]
public sealed class AuthProfileTests
{
    [Fact]
    public void Global_profile_does_not_require_tenancy_context()
    {
        AuthProfile profile = AuthProfile.Global("global");

        ModuleCompositionValidationResult result = ModuleCompositionValidator.Validate(new ModuleCompositionSnapshot(
            selectedProfiles: [new SelectedModuleProfile(profile.Descriptor)]));

        Assert.True(result.IsValid);
        Assert.False(profile.RequiresTenantContext);
        Assert.Equal("global", profile.GlobalScopeId);
        Assert.Contains(profile.Descriptor.Provides, feature => feature.Id == AuthCompositionFeatures.GlobalScope);
    }

    [Fact]
    public void Tenant_scoped_profile_requires_tenancy_context()
    {
        AuthProfile profile = AuthProfile.TenantScoped();

        ModuleCompositionValidationResult result = ModuleCompositionValidator.Validate(new ModuleCompositionSnapshot(
            selectedProfiles: [new SelectedModuleProfile(profile.Descriptor)]));

        Assert.False(result.IsValid);
        Assert.Contains("tenancy.context", Assert.Single(result.Errors), StringComparison.Ordinal);
    }

    [Fact]
    public void Tenant_scoped_profile_is_satisfied_by_tenancy_profile()
    {
        AuthProfile profile = AuthProfile.TenantScoped();

        ModuleCompositionValidationResult result = ModuleCompositionValidator.Validate(new ModuleCompositionSnapshot(
            selectedProfiles:
            [
                new SelectedModuleProfile(TenancyProfiles.Default),
                new SelectedModuleProfile(profile.Descriptor)
            ]));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Tenancy_profile_advertises_header_resolution_separately_from_context()
    {
        Assert.Contains(TenancyProfiles.Default.Provides, feature => feature.Id == TenancyCompositionFeatures.Context);
        Assert.Contains(TenancyProfiles.Default.Provides, feature => feature.Id == TenancyCompositionFeatures.HeaderResolution);
    }
}
