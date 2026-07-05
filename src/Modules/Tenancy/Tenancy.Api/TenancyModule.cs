namespace Tenancy.Api;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Shared.Api.Modules;
using Shared.Api.Observability;
using Shared.Api.Tenancy;
using Shared.ModuleComposition;
using Shared.Tenancy;
using Tenancy.Contracts;

public sealed class TenancyModule : IModule
{
    public string Name => TenancyModuleMetadata.Name;

    public void AddServices(IHostApplicationBuilder builder)
    {
        builder.SelectModuleProfile(TenancyProfiles.Default, "Tenancy.Api");
        builder.Services.Configure<TenantOptions>(builder.Configuration.GetSection(TenantOptions.SectionName));
        builder.Services.PostConfigure<TenantOptions>(options => options.Enabled = true);
        builder.Services.Replace(ServiceDescriptor.Scoped<TenantContext, TenantContext>());
        builder.Services.Replace(ServiceDescriptor.Scoped<ITenantContext>(provider => provider.GetRequiredService<TenantContext>()));
        builder.Services.Replace(ServiceDescriptor.Scoped<ITenantContextAccessor>(provider => provider.GetRequiredService<TenantContext>()));
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/tenants")
            .WithModuleName(this.Name)
            .WithTags("Tenancy");

        group.MapGet("/current", (ITenantContext tenantContext) =>
            Results.Ok(new { tenantContext.TenantId, tenantContext.IsEnabled }))
            .RequireTenant();
    }
}
