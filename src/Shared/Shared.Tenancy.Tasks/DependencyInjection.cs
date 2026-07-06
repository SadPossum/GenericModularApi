namespace Shared.Tenancy.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Shared.ModuleComposition;
using Shared.Tasks;
using Shared.Tasks.Infrastructure;
using Shared.Tenancy;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddTenantTaskExecutionContext(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (builder.Services.Any(descriptor => descriptor.ServiceType == typeof(TenantTaskExecutionContextRegistrationMarker)))
        {
            return builder;
        }

        builder.Services.AddSingleton<TenantTaskExecutionContextRegistrationMarker>();
        builder.RequireFeature(new RequiredCompositionFeature(
            TenancyCompositionFeatures.Context,
            "Shared.Tenancy.Tasks",
            optional: false,
            reason: "Tenant task execution context needs an ITenantContextAccessor provider."));
        builder.ProvideFeature(TasksCompositionFeatures.TenantScopeProvided("Shared.Tenancy.Tasks"));
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Scoped<ITaskExecutionContextContributor, TenantTaskExecutionContextContributor>());

        return builder;
    }

    private sealed class TenantTaskExecutionContextRegistrationMarker;
}
