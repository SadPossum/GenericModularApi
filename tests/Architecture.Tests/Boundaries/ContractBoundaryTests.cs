namespace Architecture.Tests;

using System.Reflection;
using Xunit;

[Trait("Category", "Architecture")]
public sealed class ContractBoundaryTests
{
    [Fact]
    public void Public_module_contract_apis_do_not_expose_other_module_types()
    {
        string[] offenders = ArchitectureCatalog.ModuleProjects
            .Where(project => project.Kind is ModuleProjectKind.Contracts or ModuleProjectKind.AdminContracts)
            .SelectMany(project => project.Assembly.ExportedTypes
                .SelectMany(type => GetPublicApiTypes(type)
                    .Where(referencedType => IsOtherModuleType(project.ModulePrefix, referencedType))
                    .Select(referencedType => $"{project.ProjectName}:{type.FullName}->{referencedType.FullName}")))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Empty(offenders);
    }

    private static IEnumerable<Type> GetPublicApiTypes(Type type)
    {
        const BindingFlags Flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        if (type.BaseType is not null)
        {
            foreach (Type referencedType in ExpandType(type.BaseType))
            {
                yield return referencedType;
            }
        }

        foreach (Type interfaceType in type.GetInterfaces())
        {
            foreach (Type referencedType in ExpandType(interfaceType))
            {
                yield return referencedType;
            }
        }

        foreach (FieldInfo field in type.GetFields(Flags))
        {
            foreach (Type referencedType in ExpandType(field.FieldType))
            {
                yield return referencedType;
            }
        }

        foreach (PropertyInfo property in type.GetProperties(Flags))
        {
            foreach (Type referencedType in ExpandType(property.PropertyType))
            {
                yield return referencedType;
            }
        }

        foreach (EventInfo eventInfo in type.GetEvents(Flags))
        {
            foreach (Type referencedType in ExpandType(eventInfo.EventHandlerType!))
            {
                yield return referencedType;
            }
        }

        foreach (ConstructorInfo constructor in type.GetConstructors(Flags))
        {
            foreach (ParameterInfo parameter in constructor.GetParameters())
            {
                foreach (Type referencedType in ExpandType(parameter.ParameterType))
                {
                    yield return referencedType;
                }
            }
        }

        foreach (MethodInfo method in type.GetMethods(Flags))
        {
            foreach (Type referencedType in ExpandType(method.ReturnType))
            {
                yield return referencedType;
            }

            foreach (ParameterInfo parameter in method.GetParameters())
            {
                foreach (Type referencedType in ExpandType(parameter.ParameterType))
                {
                    yield return referencedType;
                }
            }
        }
    }

    private static IEnumerable<Type> ExpandType(Type type)
    {
        if (type.IsByRef || type.IsPointer || type.IsArray)
        {
            foreach (Type referencedType in ExpandType(type.GetElementType()!))
            {
                yield return referencedType;
            }

            yield break;
        }

        yield return type;

        if (!type.IsGenericType)
        {
            yield break;
        }

        foreach (Type argument in type.GetGenericArguments())
        {
            foreach (Type referencedType in ExpandType(argument))
            {
                yield return referencedType;
            }
        }
    }

    private static bool IsOtherModuleType(string modulePrefix, Type type)
    {
        string? assemblyName = type.Assembly.GetName().Name;
        return assemblyName is not null &&
               ArchitectureCatalog.ModulePrefixes.Any(prefix => assemblyName.StartsWith(prefix + ".", StringComparison.Ordinal)) &&
               !assemblyName.StartsWith(modulePrefix + ".", StringComparison.Ordinal);
    }
}
