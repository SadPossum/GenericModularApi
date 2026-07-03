namespace Shared.Infrastructure.Observability;

using System.Text;
using Shared.Application.Messaging;

internal static class ModuleNameResolver
{
    public static string FromType(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        string assemblyName = type.Assembly.GetName().Name ?? "unknown";
        return FromAssemblyName(assemblyName);
    }

    internal static string FromAssemblyName(string assemblyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(assemblyName);

        int separatorIndex = assemblyName.IndexOf('.', StringComparison.Ordinal);
        string prefix = separatorIndex > 0 ? assemblyName[..separatorIndex] : assemblyName;

        return IntegrationEventNaming.NormalizeModuleName(ToKebabCase(prefix));
    }

    private static string ToKebabCase(string value)
    {
        StringBuilder builder = new(value.Length);

        for (int index = 0; index < value.Length; index++)
        {
            char character = value[index];
            if (char.IsUpper(character))
            {
                if (ShouldInsertHyphen(value, index))
                {
                    builder.Append('-');
                }

                builder.Append(char.ToLowerInvariant(character));
                continue;
            }

            builder.Append(char.ToLowerInvariant(character));
        }

        return builder.ToString();
    }

    private static bool ShouldInsertHyphen(string value, int index)
    {
        if (index == 0)
        {
            return false;
        }

        char previous = value[index - 1];
        if (char.IsLower(previous) || char.IsDigit(previous))
        {
            return true;
        }

        return char.IsUpper(previous) &&
               index + 1 < value.Length &&
               char.IsLower(value[index + 1]);
    }
}
