namespace Shared.FileManagement.LocalStorage;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shared.FileManagement;
using Shared.ModuleComposition;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddLocalFileStorage(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        IConfigurationSection fileManagement = builder.Configuration.GetSection(FileManagementOptions.SectionName);
        FileManagementOptions fileManagementOptions = fileManagement.Get<FileManagementOptions>() ?? new FileManagementOptions();
        ValidateFileManagementOptions(fileManagementOptions);

        if (!fileManagementOptions.Enabled)
        {
            return builder;
        }

        if (fileManagementOptions.Provider != FileStorageProvider.LocalStorage)
        {
            return builder;
        }

        if (builder.Services.Any(descriptor => descriptor.ServiceType == typeof(LocalFileStorageRegistrationMarker)))
        {
            return builder;
        }

        IConfigurationSection localStorage = builder.Configuration.GetSection(LocalFileStorageOptions.SectionName);
        LocalFileStorageOptions localStorageOptions = localStorage.Get<LocalFileStorageOptions>() ?? new LocalFileStorageOptions();
        ValidateLocalStorageOptions(localStorageOptions);

        builder.Services.AddSingleton<LocalFileStorageRegistrationMarker>();
        builder.ProvideFeature(FileManagementCompositionFeatures.StorageProvided("Shared.FileManagement.LocalStorage"));
        builder.Services.AddSingleton<IFileStorage, LocalFileStorage>();
        builder.Services
            .AddOptions<FileManagementOptions>()
            .Bind(fileManagement)
            .ValidateOnStart();
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<FileManagementOptions>, FileManagementOptionsValidator>());
        builder.Services
            .AddOptions<LocalFileStorageOptions>()
            .Bind(localStorage)
            .ValidateOnStart();
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<LocalFileStorageOptions>, LocalFileStorageOptionsValidator>());

        return builder;
    }

    private static void ValidateFileManagementOptions(FileManagementOptions options)
    {
        ValidateOptionsResult result = new FileManagementOptionsValidator().Validate(name: null, options);
        if (result.Failed)
        {
            throw new OptionsValidationException(
                FileManagementOptions.SectionName,
                typeof(FileManagementOptions),
                result.Failures);
        }
    }

    private static void ValidateLocalStorageOptions(LocalFileStorageOptions options)
    {
        ValidateOptionsResult result = new LocalFileStorageOptionsValidator().Validate(name: null, options);
        if (result.Failed)
        {
            throw new OptionsValidationException(
                LocalFileStorageOptions.SectionName,
                typeof(LocalFileStorageOptions),
                result.Failures);
        }
    }

    private sealed class LocalFileStorageRegistrationMarker;
}
