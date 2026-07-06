namespace Shared.Tenancy.Caching;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Shared.Caching;
using Shared.Caching.Infrastructure;
using Shared.ModuleComposition;
using Shared.Tenancy;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddTenantCaching(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (builder.Services.Any(descriptor => descriptor.ServiceType == typeof(TenantCachingRegistrationMarker)))
        {
            return builder;
        }

        builder.Services.AddSingleton<TenantCachingRegistrationMarker>();
        builder.RequireFeature(new RequiredCompositionFeature(
            TenancyCompositionFeatures.Context,
            "Shared.Tenancy.Caching",
            optional: false,
            reason: "Tenant cache scope resolution needs an ITenantContext provider."));
        builder.ProvideFeature(CachingCompositionFeatures.TenantScopeProvided("Shared.Tenancy.Caching"));
        builder.Services.Replace(ServiceDescriptor.Scoped<ICacheScopeValueResolver, TenantCacheScopeValueResolver>());

        return builder;
    }

    private sealed class TenantCachingRegistrationMarker;
}
