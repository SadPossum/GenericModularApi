namespace Shared.Administration.Cli;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shared.Administration;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedAdministrationCli(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSharedAdministration();
        services.TryAddSingleton<AdminCliGlobalOptions>();
        services.TryAddSingleton<AdminCliExecutor>();

        return services;
    }
}
