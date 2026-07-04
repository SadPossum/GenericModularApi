namespace Shared.Infrastructure;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Application.Events.Infrastructure;
using Shared.Cqrs.Infrastructure;
using Shared.Runtime.Infrastructure;
using Shared.Tenancy.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddSharedInfrastructure(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (builder.Services.Any(descriptor => descriptor.ServiceType == typeof(SharedInfrastructureRegistrationMarker)))
        {
            return builder;
        }

        builder.AddTenancyInfrastructure();
        builder.AddRuntimeInfrastructure();
        builder.AddApplicationEventsInfrastructure();
        builder.AddCqrsInfrastructure();
        builder.Services.AddSingleton<SharedInfrastructureRegistrationMarker>();

        return builder;
    }

    private sealed class SharedInfrastructureRegistrationMarker;
}
