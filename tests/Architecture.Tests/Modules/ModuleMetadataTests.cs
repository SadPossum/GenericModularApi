namespace Architecture.Tests;

using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Messaging;
using Shared.Application.Modules;
using Xunit;

[Trait("Category", "Architecture")]
public sealed partial class ModuleMetadataTests
{
    [Fact]
    public void Module_metadata_descriptors_use_stable_contract_names()
    {
        IReadOnlyList<ModuleDescriptor> descriptors = ArchitectureCatalog.ModuleDescriptors;

        Assert.Equal(descriptors.Count, descriptors.Select(descriptor => descriptor.Name).Distinct().Count());

        foreach (ModuleDescriptor descriptor in descriptors)
        {
            Assert.Equal(descriptor.Name, IntegrationEventNaming.NormalizeModuleName(descriptor.Name));
            string permissionPrefix = descriptor.AdminSurfaceName ?? descriptor.Name;

            if (descriptor.Schema is not null)
            {
                Assert.Equal(descriptor.Schema, IntegrationEventNaming.NormalizeModuleName(descriptor.Schema));
            }

            Assert.All(
                descriptor.Permissions,
                permission => Assert.StartsWith($"{permissionPrefix}.", permission.Code, StringComparison.Ordinal));
            Assert.All(
                descriptor.PublishedEvents,
                integrationEvent =>
                {
                    Assert.Equal(integrationEvent.EventType, IntegrationEventNaming.NormalizeEventName(integrationEvent.EventType));
                    Assert.Equal(
                        IntegrationEventNaming.CreateSubject("gma", descriptor.Name, integrationEvent.EventType, integrationEvent.Version),
                        integrationEvent.Subject);
                });
            Assert.All(
                descriptor.Subscriptions,
                subscription =>
                {
                    Assert.Equal(subscription.ProducerModule, IntegrationEventNaming.NormalizeModuleName(subscription.ProducerModule));
                    Assert.Equal(subscription.EventType, IntegrationEventNaming.NormalizeEventName(subscription.EventType));
                    Assert.Equal(subscription.Subject, IntegrationEventNaming.NormalizeSubject(subscription.Subject));
                    Assert.Equal(subscription.HandlerName, IntegrationEventNaming.NormalizeHandlerName(subscription.HandlerName));
                });
            Assert.All(
                descriptor.CacheEntries,
                cacheEntry => Assert.DoesNotContain(' ', cacheEntry.Name));
        }
    }

    [Fact]
    public void Module_metadata_descriptors_match_declared_name_and_schema_members()
    {
        string[] offenders = ArchitectureCatalog.ModuleDescriptors
            .Select(descriptor =>
            {
                Type metadataType = ArchitectureCatalog.ModuleProjects
                    .Where(project => project.Kind == ModuleProjectKind.Contracts)
                    .SelectMany(project => project.Assembly.GetTypes())
                    .Single(type =>
                        type.Name.EndsWith("ModuleMetadata", StringComparison.Ordinal) &&
                        type.GetProperty("Descriptor")?.GetValue(null) is ModuleDescriptor item &&
                        string.Equals(item.Name, descriptor.Name, StringComparison.Ordinal));

                string? name = metadataType.GetField("Name")?.GetRawConstantValue() as string;
                string? schema = metadataType.GetField("Schema")?.GetRawConstantValue() as string ??
                                 metadataType.GetProperty("Schema")?.GetValue(null) as string;
                string? adminSurfaceName = metadataType.GetField("AdminSurfaceName")?.GetRawConstantValue() as string ??
                                           metadataType.GetProperty("AdminSurfaceName")?.GetValue(null) as string;

                return string.Equals(name, descriptor.Name, StringComparison.Ordinal) &&
                       string.Equals(schema, descriptor.Schema, StringComparison.Ordinal) &&
                       string.Equals(adminSurfaceName, descriptor.AdminSurfaceName, StringComparison.Ordinal)
                    ? null
                    : $"{metadataType.FullName}: Name={name ?? "<missing>"}, Schema={schema ?? "<null>"}, AdminSurfaceName={adminSurfaceName ?? "<null>"}";
            })
            .Where(offender => offender is not null)
            .Select(offender => offender!)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Empty(offenders);
    }

    [Fact]
    public void Architecture_catalog_module_descriptors_match_contract_metadata()
    {
        ModuleDescriptor[] contractMetadataDescriptors = ArchitectureCatalog.ModuleProjects
            .Where(project => project.Kind == ModuleProjectKind.Contracts)
            .SelectMany(project => project.Assembly
                .GetTypes()
                .Where(type => type.Name.EndsWith("ModuleMetadata", StringComparison.Ordinal))
                .Select(type => type.GetProperty("Descriptor", BindingFlags.Public | BindingFlags.Static)?.GetValue(null))
                .OfType<ModuleDescriptor>())
            .OrderBy(descriptor => descriptor.Name, StringComparer.Ordinal)
            .ToArray();
        ModuleDescriptor[] catalogDescriptors = ArchitectureCatalog.ModuleDescriptors
            .OrderBy(descriptor => descriptor.Name, StringComparer.Ordinal)
            .ToArray();

        string[] metadataOnly = contractMetadataDescriptors
            .Where(metadata => !catalogDescriptors.Any(catalog => string.Equals(catalog.Name, metadata.Name, StringComparison.Ordinal)))
            .Select(metadata => $"metadata-only:{metadata.Name}")
            .ToArray();
        string[] catalogOnly = catalogDescriptors
            .Where(catalog => !contractMetadataDescriptors.Any(metadata => string.Equals(metadata.Name, catalog.Name, StringComparison.Ordinal)))
            .Select(catalog => $"catalog-only:{catalog.Name}")
            .ToArray();

        Assert.Empty(metadataOnly
            .Concat(catalogOnly)
            .Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Module_persistence_schema_constants_match_module_metadata()
    {
        Dictionary<string, ModuleDescriptor> descriptors = ArchitectureCatalog.ModuleDescriptors
            .Where(descriptor => descriptor.Schema is not null)
            .ToDictionary(descriptor => descriptor.Name, StringComparer.Ordinal);
        string[] offenders = ArchitectureCatalog.ModuleProjects
            .Where(project => project.Kind == ModuleProjectKind.Persistence)
            .Select(project => new
            {
                Project = project,
                ModuleName = ToModuleName(project.ModulePrefix),
            })
            .Where(item => descriptors.ContainsKey(item.ModuleName))
            .Select(item =>
            {
                Type? migrationsType = item.Project.Assembly
                    .GetTypes()
                    .SingleOrDefault(type => type.Name.EndsWith("Migrations", StringComparison.Ordinal));
                string? schema = migrationsType
                    ?.GetField("Schema")
                    ?.GetRawConstantValue() as string;

                return string.Equals(schema, descriptors[item.ModuleName].Schema, StringComparison.Ordinal)
                    ? null
                    : $"{item.Project.ProjectName}: Schema={schema ?? "<missing>"} expected {descriptors[item.ModuleName].Schema}";
            })
            .Where(offender => offender is not null)
            .Select(offender => offender!)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Empty(offenders);
    }

    [Fact]
    public void Module_permission_metadata_matches_permission_code_constants()
    {
        Dictionary<string, ModuleDescriptor> descriptors = ArchitectureCatalog.ModuleDescriptors
            .ToDictionary(descriptor => descriptor.Name, StringComparer.Ordinal);
        ModulePermissionCode[] permissionCodes = ArchitectureCatalog.ModuleProjects
            .Where(project => project.Kind == ModuleProjectKind.Contracts)
            .SelectMany(project => project.Assembly
                .GetTypes()
                .Where(type => type is { IsAbstract: true, IsSealed: true } &&
                               type.Name.EndsWith("PermissionCodes", StringComparison.Ordinal))
                .SelectMany(type => type
                    .GetFields(BindingFlags.Public | BindingFlags.Static)
                    .Where(field => field is { IsLiteral: true, IsInitOnly: false } && field.FieldType == typeof(string))
                    .Select(field => new ModulePermissionCode(
                        ToModuleName(project.ModulePrefix),
                        type,
                        field,
                        field.GetRawConstantValue() as string))))
            .ToArray();

        string[] missingMetadata = permissionCodes
            .Where(permission =>
                permission.Code is null ||
                !descriptors.TryGetValue(permission.ModuleName, out ModuleDescriptor? descriptor) ||
                !descriptor.Permissions.Any(item => string.Equals(item.Code, permission.Code, StringComparison.Ordinal)))
            .Select(permission => $"{permission.ModuleName}:{permission.Type.Name}.{permission.Field.Name}={permission.Code ?? "<null>"}")
            .Order(StringComparer.Ordinal)
            .ToArray();
        string[] missingConstants = descriptors.Values
            .SelectMany(descriptor => descriptor.Permissions.Select(permission => new
            {
                descriptor.Name,
                permission.Code,
            }))
            .Where(permission => !permissionCodes.Any(code =>
                string.Equals(code.ModuleName, permission.Name, StringComparison.Ordinal) &&
                string.Equals(code.Code, permission.Code, StringComparison.Ordinal)))
            .Select(permission => $"{permission.Name}:{permission.Code}")
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Empty(missingMetadata
            .Concat(missingConstants)
            .Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Module_subscriptions_match_declared_producer_events()
    {
        Dictionary<string, ModuleDescriptor> descriptors = ArchitectureCatalog.ModuleDescriptors
            .ToDictionary(descriptor => descriptor.Name, StringComparer.Ordinal);
        string[] offenders = ArchitectureCatalog.ModuleDescriptors
            .SelectMany(consumer => consumer.Subscriptions.Select(subscription => new
            {
                Consumer = consumer.Name,
                Subscription = subscription,
            }))
            .Where(item =>
                !descriptors.TryGetValue(item.Subscription.ProducerModule, out ModuleDescriptor? producer) ||
                !producer.PublishedEvents.Any(producerEvent =>
                    string.Equals(producerEvent.EventType, item.Subscription.EventType, StringComparison.Ordinal) &&
                    string.Equals(producerEvent.Subject, item.Subscription.Subject, StringComparison.Ordinal) &&
                    producerEvent.TenantScoped == item.Subscription.TenantScoped))
            .Select(item => $"{item.Consumer}:{item.Subscription.HandlerName}->{item.Subscription.Subject}")
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Empty(offenders);
    }

    [Fact]
    public void Module_subscription_metadata_matches_application_registrations()
    {
        ServiceCollection services = new();
        IConfiguration configuration = new ConfigurationBuilder().Build();
        string[] registrationOffenders = ArchitectureCatalog.ModuleProjects
            .Where(project => project.Kind == ModuleProjectKind.Application)
            .Select(project => RegisterModuleApplication(services, configuration, project))
            .Where(offender => offender is not null)
            .Select(offender => offender!)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Empty(registrationOffenders);

        using ServiceProvider provider = services.BuildServiceProvider();

        IntegrationEventSubscription[] registeredSubscriptions = provider
            .GetService<IIntegrationEventSubscriptionRegistry>()
            ?.Subscriptions
            .OrderBy(subscription => subscription.ConsumerModule, StringComparer.Ordinal)
            .ThenBy(subscription => subscription.HandlerName, StringComparer.Ordinal)
            .ToArray() ?? [];

        ModuleSubscriptionRecord[] declaredSubscriptions = ArchitectureCatalog
            .ModuleDescriptors
            .SelectMany(descriptor => descriptor.Subscriptions.Select(subscription => new ModuleSubscriptionRecord(
                descriptor.Name,
                subscription)))
            .OrderBy(subscription => subscription.ConsumerModule, StringComparer.Ordinal)
            .ThenBy(subscription => subscription.Subscription.HandlerName, StringComparer.Ordinal)
            .ToArray();

        string[] missingRegistrations = declaredSubscriptions
            .Where(declared => !registeredSubscriptions.Any(registered => SubscriptionMatchesRegistration(declared, registered)))
            .Select(declared => $"metadata-only:{declared.ConsumerModule}:{declared.Subscription.HandlerName}:{declared.Subscription.Subject}")
            .ToArray();
        string[] missingMetadata = registeredSubscriptions
            .Where(registered => !declaredSubscriptions.Any(declared => SubscriptionMatchesRegistration(declared, registered)))
            .Select(registered => $"registration-only:{registered.ConsumerModule}:{registered.HandlerName}:{registered.Subject}")
            .ToArray();

        Assert.Empty(missingRegistrations
            .Concat(missingMetadata)
            .Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Module_published_event_metadata_matches_integration_event_contracts()
    {
        Dictionary<string, ModuleDescriptor> descriptors = ArchitectureCatalog.ModuleDescriptors
            .ToDictionary(descriptor => descriptor.Name, StringComparer.Ordinal);
        IntegrationEventContract[] eventContracts = ArchitectureCatalog.ModuleProjects
            .Where(project => project.Kind == ModuleProjectKind.Contracts)
            .SelectMany(project => project.Assembly
                .GetTypes()
                .Where(type => !type.IsAbstract && typeof(IIntegrationEvent).IsAssignableFrom(type))
                .Select(type => CreateIntegrationEventContract(ToModuleName(project.ModulePrefix), type)))
            .ToArray();

        string[] missingMetadata = eventContracts
            .Where(contract =>
                !descriptors.TryGetValue(contract.ModuleName, out ModuleDescriptor? descriptor) ||
                !descriptor.PublishedEvents.Any(publishedEvent =>
                    string.Equals(publishedEvent.EventType, contract.EventName, StringComparison.Ordinal) &&
                    publishedEvent.Version == contract.Version &&
                    string.Equals(publishedEvent.Subject, contract.Subject, StringComparison.Ordinal)))
            .Select(contract => $"{contract.ModuleName}:{contract.EventType.FullName}:{contract.Subject}")
            .Order(StringComparer.Ordinal)
            .ToArray();
        string[] missingContracts = descriptors.Values
            .SelectMany(descriptor => descriptor.PublishedEvents.Select(publishedEvent => new
            {
                ModuleName = descriptor.Name,
                PublishedEvent = publishedEvent,
            }))
            .Where(item => !eventContracts.Any(contract =>
                string.Equals(contract.ModuleName, item.ModuleName, StringComparison.Ordinal) &&
                string.Equals(contract.EventName, item.PublishedEvent.EventType, StringComparison.Ordinal) &&
                contract.Version == item.PublishedEvent.Version &&
                string.Equals(contract.Subject, item.PublishedEvent.Subject, StringComparison.Ordinal)))
            .Select(item => $"{item.ModuleName}:{item.PublishedEvent.EventType}:{item.PublishedEvent.Subject}")
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Empty(missingMetadata
            .Concat(missingContracts)
            .Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Module_cache_metadata_tags_are_referenced_by_module_cache_helpers()
    {
        string repositoryRoot = FindRepositoryRoot();
        string[] offenders = ArchitectureCatalog.ModuleDescriptors
            .SelectMany(descriptor => descriptor.CacheEntries.SelectMany(cacheEntry => cacheEntry.Tags.Select(tag => new
            {
                ModuleName = descriptor.Name,
                CacheEntryName = cacheEntry.Name,
                Tag = tag,
            })))
            .Where(item =>
            {
                string moduleRoot = Path.Combine(
                    repositoryRoot,
                    "src",
                    "Modules",
                    ToProjectPrefix(item.ModuleName));
                string[] sourceFiles = Directory.Exists(moduleRoot)
                    ? Directory.EnumerateFiles(moduleRoot, "*.cs", SearchOption.AllDirectories)
                        .Where(path => !HasIgnoredPathSegment(path) && !IsGeneratedMigrationSource(path))
                        .ToArray()
                    : [];
                string? tagConstantName = FindStringConstantName(sourceFiles, item.Tag);

                return !sourceFiles.Any(path =>
                {
                    string source = File.ReadAllText(path);

                    return source.Contains(item.Tag, StringComparison.Ordinal) ||
                           (tagConstantName is not null && source.Contains($".{tagConstantName}", StringComparison.Ordinal));
                });
            })
            .Select(item => $"{item.ModuleName}:{item.CacheEntryName}:{item.Tag}")
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Empty(offenders);
    }

    private static IntegrationEventContract CreateIntegrationEventContract(string moduleName, Type eventType)
    {
        IIntegrationEvent integrationEvent = CreateIntegrationEvent(eventType);
        string subject = $"gma.{moduleName}.{integrationEvent.EventName}.v{integrationEvent.Version}";

        return new(moduleName, eventType, integrationEvent.EventName, integrationEvent.Version, subject);
    }

    private static string? RegisterModuleApplication(
        IServiceCollection services,
        IConfiguration configuration,
        ModuleProject project)
    {
        Type? dependencyInjection = project.Assembly.GetType($"{project.ModulePrefix}.Application.DependencyInjection");
        MethodInfo? registrationMethod = dependencyInjection
            ?.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(method => string.Equals(method.Name, $"Add{project.ModulePrefix}Application", StringComparison.Ordinal));

        if (registrationMethod is null)
        {
            return $"{project.ProjectName} does not expose Add{project.ModulePrefix}Application.";
        }

        ParameterInfo[] parameters = registrationMethod.GetParameters();
        object?[] arguments;
        if (parameters.Length == 1 &&
            parameters[0].ParameterType == typeof(IServiceCollection))
        {
            arguments = [services];
        }
        else if (parameters.Length == 2 &&
                 parameters[0].ParameterType == typeof(IServiceCollection) &&
                 parameters[1].ParameterType == typeof(IConfiguration))
        {
            arguments = [services, configuration];
        }
        else
        {
            arguments = [];
        }

        if (arguments.Length == 0)
        {
            return $"{project.ProjectName}.{registrationMethod.Name} has unsupported parameters.";
        }

        registrationMethod.Invoke(null, arguments);
        return null;
    }

    private static bool SubscriptionMatchesRegistration(
        ModuleSubscriptionRecord declared,
        IntegrationEventSubscription registered)
    {
        IIntegrationEvent registeredEvent = CreateIntegrationEvent(registered.EventType);

        return string.Equals(declared.ConsumerModule, registered.ConsumerModule, StringComparison.Ordinal) &&
               string.Equals(declared.Subscription.Subject, registered.Subject, StringComparison.Ordinal) &&
               string.Equals(declared.Subscription.HandlerName, registered.HandlerName, StringComparison.Ordinal) &&
               string.Equals(declared.Subscription.EventType, registeredEvent.EventName, StringComparison.Ordinal) &&
               declared.Subscription.TenantScoped == registered.TenantScoped;
    }

    private static IIntegrationEvent CreateIntegrationEvent(Type eventType)
    {
        object?[] arguments = eventType
            .GetConstructors()
            .Single()
            .GetParameters()
            .Select(parameter => CreateSampleValue(parameter.ParameterType, parameter.Name ?? string.Empty))
            .ToArray();

        return (IIntegrationEvent)Activator.CreateInstance(eventType, arguments)!;
    }

    private static object? CreateSampleValue(Type type, string parameterName)
    {
        Type effectiveType = Nullable.GetUnderlyingType(type) ?? type;

        if (effectiveType == typeof(Guid))
        {
            return Guid.Parse("f2891ca4-c28a-4779-99b4-77972ae2ca95");
        }

        if (effectiveType == typeof(string))
        {
            return parameterName switch
            {
                "tenantId" => "tenant-a",
                "sku" => "SKU-1",
                "currency" => "USD",
                "username" => "user@example.com",
                "reason" => "support request",
                "name" => "Sample item",
                _ => "sample"
            };
        }

        if (effectiveType == typeof(DateTimeOffset))
        {
            return new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        }

        if (effectiveType == typeof(int))
        {
            return 1;
        }

        if (effectiveType == typeof(decimal))
        {
            return 1m;
        }

        if (effectiveType.IsEnum)
        {
            Array values = Enum.GetValues(effectiveType);
            return values
                .Cast<object>()
                .FirstOrDefault(value => Convert.ToInt64(value, CultureInfo.InvariantCulture) != 0) ??
                   values.GetValue(0);
        }

        throw new InvalidOperationException($"No sample value is configured for integration event parameter type '{type.FullName}'.");
    }

    private static string ToProjectPrefix(string moduleName) =>
        string.Concat(
            moduleName
                .Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(part => char.ToUpperInvariant(part[0]) + part[1..]));

    private static string ToModuleName(string projectPrefix)
    {
        string withAcronymBoundaries = AcronymBoundaryPattern().Replace(projectPrefix, "$1-$2");
        return WordBoundaryPattern().Replace(withAcronymBoundaries, "$1-$2").ToLowerInvariant();
    }

    private static string? FindStringConstantName(IEnumerable<string> sourceFiles, string value) =>
        sourceFiles
            .SelectMany(path => StringConstantPattern().Matches(File.ReadAllText(path)))
            .Cast<System.Text.RegularExpressions.Match>()
            .Where(match => string.Equals(match.Groups["value"].Value, value, StringComparison.Ordinal))
            .Select(match => match.Groups["name"].Value)
            .FirstOrDefault();

    private static bool IsGeneratedMigrationSource(string sourcePath) =>
        HasPathSegment(sourcePath, "Migrations") ||
        string.Equals(Path.GetFileName(sourcePath), "ModelSnapshot.cs", StringComparison.Ordinal);

    private static bool HasIgnoredPathSegment(string path)
    {
        string[] segments = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return segments.Contains("bin", StringComparer.OrdinalIgnoreCase) ||
               segments.Contains("obj", StringComparer.OrdinalIgnoreCase);
    }

    private static bool HasPathSegment(string path, string segment)
    {
        string[] segments = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return segments.Contains(segment, StringComparer.OrdinalIgnoreCase);
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(Directory.GetCurrentDirectory());

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "GenericModularApi.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root.");
    }

    private sealed record IntegrationEventContract(
        string ModuleName,
        Type EventType,
        string EventName,
        int Version,
        string Subject);

    private sealed record ModuleSubscriptionRecord(
        string ConsumerModule,
        ModuleSubscriptionDescriptor Subscription);

    private sealed record ModulePermissionCode(
        string ModuleName,
        Type Type,
        FieldInfo Field,
        string? Code);

    [System.Text.RegularExpressions.GeneratedRegex(@"public\s+const\s+string\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*=\s*""(?<value>[^""]+)""")]
    private static partial System.Text.RegularExpressions.Regex StringConstantPattern();

    [System.Text.RegularExpressions.GeneratedRegex(@"([A-Z]+)([A-Z][a-z])")]
    private static partial System.Text.RegularExpressions.Regex AcronymBoundaryPattern();

    [System.Text.RegularExpressions.GeneratedRegex(@"([a-z0-9])([A-Z])")]
    private static partial System.Text.RegularExpressions.Regex WordBoundaryPattern();
}
