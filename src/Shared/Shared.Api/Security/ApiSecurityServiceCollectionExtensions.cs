namespace Shared.Api.Security;

using Microsoft.Extensions.DependencyInjection;

public static class ApiSecurityServiceCollectionExtensions
{
    public static IServiceCollection AddGmaApiSecurityDefaults(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddAuthentication();
        services.AddAuthorization();

        return services;
    }
}
