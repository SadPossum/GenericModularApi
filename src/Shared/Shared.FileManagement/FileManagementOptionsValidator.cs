namespace Shared.FileManagement;

using Microsoft.Extensions.Options;

public sealed class FileManagementOptionsValidator : IValidateOptions<FileManagementOptions>
{
    public ValidateOptionsResult Validate(string? name, FileManagementOptions options)
    {
        if (options.MaximumObjectBytes <= 0)
        {
            return ValidateOptionsResult.Fail(
                $"{FileManagementOptions.SectionName}:MaximumObjectBytes must be greater than zero.");
        }

        if (!Enum.IsDefined(options.Provider))
        {
            return ValidateOptionsResult.Fail(
                $"{FileManagementOptions.SectionName}:Provider is not supported.");
        }

        if (options.Enabled && options.Provider == FileStorageProvider.Unknown)
        {
            return ValidateOptionsResult.Fail(
                $"{FileManagementOptions.SectionName}:Provider is required when file management is enabled.");
        }

        string[] invalidContentTypes = options.AllowedContentTypes
            .Where(contentType => !FileStorageMetadata.IsValidContentType(contentType))
            .ToArray();

        return invalidContentTypes.Length == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(
                $"{FileManagementOptions.SectionName}:AllowedContentTypes contains invalid content types.");
    }
}
