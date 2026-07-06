namespace Files.Application;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Shared.Application.Composition;
using Shared.FileManagement;

public static class DependencyInjection
{
    public static IServiceCollection AddFilesApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<FileManagementOptions>()
            .Bind(configuration.GetSection(FileManagementOptions.SectionName))
            .ValidateOnStart();
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<FileManagementOptions>, FileManagementOptionsValidator>());
        services.AddApplicationServicesFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
