namespace Auth.Infrastructure;

using Auth.Domain.Services;
using Auth.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Modules;

public class AuthInfrastructureModule : IServiceModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration) =>
        services.AddScoped<ICryptographyService, CryptographyService>()
            .AddScoped<ITokenProviderService, JwtProviderService>()
            .AddScoped<ITokenValidatorService, JwtValidatorService>();
}
