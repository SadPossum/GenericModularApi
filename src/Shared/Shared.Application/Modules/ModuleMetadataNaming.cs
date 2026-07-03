namespace Shared.Application.Modules;

using Shared.Application.Messaging;

internal static class ModuleMetadataNaming
{
    public const int DottedCodeMaxLength = 256;

    public static string NormalizeModuleName(string moduleName, string parameterName) =>
        IntegrationEventNaming.NormalizeModuleName(moduleName, parameterName);

    public static string NormalizeDottedCode(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);

        string normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length > DottedCodeMaxLength)
        {
            throw new ArgumentException(
                $"{parameterName} must be {DottedCodeMaxLength} characters or fewer.",
                parameterName);
        }

        string[] segments = normalized.Split('.');
        if (segments.Length < 2)
        {
            throw new ArgumentException($"{parameterName} must be a dot-separated code.", parameterName);
        }

        foreach (string segment in segments)
        {
            _ = IntegrationEventNaming.NormalizeModuleName(segment, parameterName);
        }

        return normalized;
    }

    public static string NormalizeCacheScope(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);

        string normalized = value.Trim().ToLowerInvariant();
        return normalized is "tenant" or "global"
            ? normalized
            : throw new ArgumentException($"{parameterName} must be either 'tenant' or 'global'.", parameterName);
    }

    public static IReadOnlyList<T> CopyRequiredList<T>(IReadOnlyList<T>? items, string parameterName)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(items, parameterName);

        T[] values = items.ToArray();
        if (values.Any(item => item is null))
        {
            throw new ArgumentException($"{parameterName} must not contain null entries.", parameterName);
        }

        return Array.AsReadOnly(values);
    }

    public static void EnsureUnique<T>(
        IEnumerable<T> items,
        Func<T, string> keySelector,
        string description)
    {
        string? duplicate = items
            .Select(keySelector)
            .GroupBy(key => key, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1)
            ?.Key;

        if (duplicate is not null)
        {
            throw new ArgumentException($"Duplicate module metadata {description} '{duplicate}'.");
        }
    }
}
