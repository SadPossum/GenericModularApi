namespace Shared.Application.Events.Infrastructure;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Shared.Application.Events;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddApplicationEventsInfrastructure(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (builder.Services.Any(descriptor => descriptor.ServiceType == typeof(ApplicationEventsInfrastructureRegistrationMarker)))
        {
            return builder;
        }

        builder.Services.AddSingleton<ApplicationEventsInfrastructureRegistrationMarker>();
        builder.Services.TryAddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        return builder;
    }

    private sealed class ApplicationEventsInfrastructureRegistrationMarker;
}
