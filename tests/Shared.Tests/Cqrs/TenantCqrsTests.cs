namespace Shared.Tests;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Cqrs.Infrastructure;
using Shared.ModuleComposition;
using Shared.Observability;
using Shared.Tenancy;
using Shared.Tenancy.Cqrs;
using Shared.Tenancy.Infrastructure;
using Xunit;

[Trait("Category", "Unit")]
public sealed class TenantCqrsTests
{
    [Fact]
    public void Tenant_cqrs_logging_registers_scope_contributor()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        builder.Configuration["Tenancy:Enabled"] = "true";

        builder.AddTenancyInfrastructure();
        builder.AddTenantCqrsLogging();
        ModuleCompositionValidationResult validation = builder.ValidateModuleComposition();

        using IHost host = builder.Build();
        using IServiceScope scope = host.Services.CreateScope();
        ICqrsLogScopeContributor contributor = Assert.Single(scope.ServiceProvider.GetServices<ICqrsLogScopeContributor>());
        scope.ServiceProvider.GetRequiredService<ITenantContextAccessor>().SetTenant("tenant-a");

        Dictionary<string, object?> scopeProperties = [];
        contributor.Enrich(
            new CqrsLogScopeContext("auth", "RegisterMemberCommand", typeof(TestCommand), CqrsRequestKind.Command),
            scopeProperties);

        Assert.True(validation.IsValid, validation.Report);
        Assert.Equal("tenant-a", scopeProperties[ObservabilityLogPropertyNames.TenantId]);
    }

    [Fact]
    public void Tenant_cqrs_logging_requires_tenancy_context_provider()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();

        builder.AddTenantCqrsLogging();

        ModuleCompositionValidationException exception = Assert.Throws<ModuleCompositionValidationException>(
            () => builder.ValidateModuleComposition());

        Assert.Contains("tenancy.context", exception.Message, StringComparison.Ordinal);
        Assert.Contains(nameof(ITenantContext), exception.Message, StringComparison.Ordinal);
    }

    private sealed record TestCommand;
}
