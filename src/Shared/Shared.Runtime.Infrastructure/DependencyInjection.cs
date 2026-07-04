namespace Shared.Runtime.Infrastructure;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Shared.Runtime.Identity;
using Shared.Runtime.Infrastructure.Identity;
using Shared.Runtime.Infrastructure.Time;
using Shared.Runtime.Time;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddRuntimeInfrastructure(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (builder.Services.Any(descriptor => descriptor.ServiceType == typeof(RuntimeInfrastructureRegistrationMarker)))
        {
            return builder;
        }

        builder.Services.AddSingleton<RuntimeInfrastructureRegistrationMarker>();
        builder.Services.TryAddSingleton<IIdGenerator, GuidIdGenerator>();
        builder.Services.TryAddSingleton<ISystemClock, SystemClock>();

        return builder;
    }

    private sealed class RuntimeInfrastructureRegistrationMarker;
}
