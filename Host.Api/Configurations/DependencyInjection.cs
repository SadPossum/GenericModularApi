namespace Host.Api.Configurations;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddCors();

        services.AddAuthorization();

        return services;
    }

    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {

        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(setup => setup.SwaggerDoc("v1", new OpenApiInfo()
        {
            Description = "This is a simple implementation of a Minimal Api in Asp.Net 7 Core",
            Title = "Generic Modular Api",
            Version = "v1",
        }));

        return services;
    }
}
