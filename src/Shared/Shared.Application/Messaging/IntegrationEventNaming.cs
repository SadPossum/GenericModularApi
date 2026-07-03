namespace Shared.Application.Messaging;

using System.Globalization;

public static class IntegrationEventNaming
{
    public static string NormalizeSubjectPrefix(string subjectPrefix, string parameterName = "subjectPrefix") =>
        NormalizeKebabSegment(subjectPrefix, "subject prefix", parameterName);

    public static string NormalizeModuleName(string moduleName, string parameterName = "moduleName") =>
        NormalizeKebabSegment(moduleName, "module name", parameterName);

    public static string NormalizeEventName(string eventName, string parameterName = "eventName") =>
        NormalizeKebabSegment(eventName, "event name", parameterName);

    public static bool TryNormalizeEventName(string? eventName, out string normalized)
    {
        if (string.IsNullOrWhiteSpace(eventName))
        {
            normalized = string.Empty;
            return false;
        }

        normalized = eventName.Trim().ToLowerInvariant();
        return IsKebabSegment(normalized);
    }

    public static string NormalizeHandlerName(string handlerName, string parameterName = "handlerName") =>
        NormalizeKebabSegment(handlerName, "handler name", parameterName);

    public static string CreateSubject(
        string subjectPrefix,
        string moduleName,
        string eventName,
        int version)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(version, 1);

        return string.Create(
            CultureInfo.InvariantCulture,
            $"{NormalizeSubjectPrefix(subjectPrefix)}.{NormalizeModuleName(moduleName)}.{NormalizeEventName(eventName)}.v{version}");
    }

    public static string NormalizeSubject(string subject, string parameterName = "subject")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subject, parameterName);

        string normalized = subject.Trim().ToLowerInvariant();
        string[] parts = normalized.Split('.');

        if (parts.Length != 4 ||
            !parts[3].StartsWith('v') ||
            !int.TryParse(parts[3][1..], NumberStyles.None, CultureInfo.InvariantCulture, out int version) ||
            parts[3] != string.Create(CultureInfo.InvariantCulture, $"v{version}"))
        {
            throw new ArgumentException(
                $"{parameterName} must use the gma.<module>.<event>.v<version> integration-event subject shape.",
                parameterName);
        }

        return CreateSubject(parts[0], parts[1], parts[2], version);
    }

    private static string NormalizeKebabSegment(string value, string description, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);

        string normalized = value.Trim().ToLowerInvariant();
        if (!IsKebabSegment(normalized))
        {
            throw new ArgumentException(
                $"{parameterName} must be a lowercase kebab-case {description}.",
                parameterName);
        }

        return normalized;
    }

    private static bool IsKebabSegment(string value)
    {
        if (value.Length == 0 ||
            value[0] == '-' ||
            value[^1] == '-' ||
            value.Contains("--", StringComparison.Ordinal))
        {
            return false;
        }

        return value.All(character =>
            char.IsAsciiLetterOrDigit(character) ||
            character == '-');
    }
}
