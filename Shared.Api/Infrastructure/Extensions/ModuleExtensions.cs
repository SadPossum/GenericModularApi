namespace Shared.Api.Infrastructure.Extensions;

using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Api.Infrastructure.Modules;
using Shared.Infrastructure.Modules;

public static class ModuleExtensions
{
    public static IEndpointRouteBuilder MapModuleEndpoints<TModule>(this IEndpointRouteBuilder endpointRouteBuilder)
        where TModule : IMinimalApiModule, new()
    {
        TModule module = new();

        module.AddRoutes(endpointRouteBuilder);

        return endpointRouteBuilder;
    }

    public static IServiceCollection ConfigureModuleServices<TModule>(this IServiceCollection services, IConfiguration configuration)
        where TModule : IServiceModule, new()
    {
        TModule module = new();

        module.ConfigureServices(services, configuration);

        return services;
    }
}
