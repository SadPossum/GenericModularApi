namespace Shared.Tenancy.Cqrs;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Shared.Cqrs.Infrastructure;
using Shared.ModuleComposition;
using Shared.Tenancy;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddTenantCqrsLogging(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (builder.Services.Any(descriptor => descriptor.ServiceType == typeof(TenantCqrsLoggingRegistrationMarker)))
        {
            return builder;
        }

        builder.Services.AddSingleton<TenantCqrsLoggingRegistrationMarker>();
        builder.RequireFeature(new RequiredCompositionFeature(
            TenancyCompositionFeatures.Context,
            "Shared.Tenancy.Cqrs",
            optional: false,
            reason: "Tenant CQRS logging needs an ITenantContext provider."));
        builder.ProvideFeature(TenancyCqrsCompositionFeatures.LogScopeProvided("Shared.Tenancy.Cqrs"));
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Scoped<ICqrsLogScopeContributor, TenantCqrsLogScopeContributor>());

        return builder;
    }

    private sealed class TenantCqrsLoggingRegistrationMarker;
}
