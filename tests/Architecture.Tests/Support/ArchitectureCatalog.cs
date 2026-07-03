namespace Architecture.Tests;

using System.Reflection;
using Administration.AdminCli;
using Administration.AdminApi;
using Administration.Application;
using Administration.Contracts;
using Administration.Persistence;
using Auth.Admin.Contracts;
using Auth.AdminCli;
using Auth.AdminApi;
using Auth.Api;
using Auth.Contracts;
using Auth.Domain.Aggregates;
using Auth.Infrastructure;
using Auth.Infrastructure.JwtBearer;
using Auth.Persistence;
using Catalog.AdminCli;
using Catalog.Admin.Contracts;
using Catalog.AdminApi;
using Catalog.Api;
using Catalog.Contracts;
using Catalog.Domain.Aggregates;
using Catalog.Persistence;
using Host.AdminApi;
using Host.AdminCli;
using Ordering.Contracts;
using Ordering.Domain.Aggregates;
using Ordering.Persistence;
using Shared.Application.Modules;
using TaskRuntime.Admin.Contracts;
using TaskRuntime.AdminCli;
using TaskRuntime.AdminApi;
using TaskRuntime.Contracts;
using TaskRuntime.Persistence;
using TaskSamples.Application;
using TaskSamples.Contracts;
using Tenancy.Api;
using Tenancy.Contracts;

internal static class ArchitectureCatalog
{
    public static IReadOnlyList<ModuleProject> ModuleProjects { get; } =
    [
        new("Administration", "Administration.AdminCli", ModuleProjectKind.AdminCli, typeof(AdministrationAdminCliModule).Assembly),
        new("Administration", "Administration.AdminApi", ModuleProjectKind.AdminApi, typeof(AdministrationAdminApiModule).Assembly),
        new("Administration", "Administration.Application", ModuleProjectKind.Application, typeof(Administration.Application.DependencyInjection).Assembly),
        new("Administration", "Administration.Contracts", ModuleProjectKind.Contracts, typeof(AdministrationModuleMetadata).Assembly),
        new("Administration", "Administration.Persistence", ModuleProjectKind.Persistence, typeof(Administration.Persistence.DependencyInjection).Assembly),

        new("Auth", "Auth.Admin.Contracts", ModuleProjectKind.AdminContracts, typeof(AuthAdminPermissions).Assembly),
        new("Auth", "Auth.AdminCli", ModuleProjectKind.AdminCli, typeof(AuthAdminCliModule).Assembly),
        new("Auth", "Auth.AdminApi", ModuleProjectKind.AdminApi, typeof(AuthAdminApiModule).Assembly),
        new("Auth", "Auth.Api", ModuleProjectKind.Api, typeof(AuthModule).Assembly),
        new("Auth", "Auth.Application", ModuleProjectKind.Application, typeof(Auth.Application.DependencyInjection).Assembly),
        new("Auth", "Auth.Contracts", ModuleProjectKind.Contracts, typeof(AuthModuleMetadata).Assembly),
        new("Auth", "Auth.Domain", ModuleProjectKind.Domain, typeof(Member).Assembly),
        new("Auth", "Auth.Infrastructure", ModuleProjectKind.Infrastructure, typeof(Auth.Infrastructure.DependencyInjection).Assembly),
        new("Auth", "Auth.Infrastructure.JwtBearer", ModuleProjectKind.Infrastructure, typeof(Auth.Infrastructure.JwtBearer.DependencyInjection).Assembly),
        new("Auth", "Auth.Persistence", ModuleProjectKind.Persistence, typeof(Auth.Persistence.DependencyInjection).Assembly),

        new("Catalog", "Catalog.Admin.Contracts", ModuleProjectKind.AdminContracts, typeof(CatalogAdminPermissions).Assembly),
        new("Catalog", "Catalog.AdminCli", ModuleProjectKind.AdminCli, typeof(CatalogAdminCliModule).Assembly),
        new("Catalog", "Catalog.AdminApi", ModuleProjectKind.AdminApi, typeof(CatalogAdminApiModule).Assembly),
        new("Catalog", "Catalog.Api", ModuleProjectKind.Api, typeof(CatalogModule).Assembly),
        new("Catalog", "Catalog.Application", ModuleProjectKind.Application, typeof(Catalog.Application.DependencyInjection).Assembly),
        new("Catalog", "Catalog.Contracts", ModuleProjectKind.Contracts, typeof(CatalogModuleMetadata).Assembly),
        new("Catalog", "Catalog.Domain", ModuleProjectKind.Domain, typeof(CatalogItem).Assembly),
        new("Catalog", "Catalog.Persistence", ModuleProjectKind.Persistence, typeof(Catalog.Persistence.DependencyInjection).Assembly),

        new("Ordering", "Ordering.Application", ModuleProjectKind.Application, typeof(Ordering.Application.DependencyInjection).Assembly),
        new("Ordering", "Ordering.Contracts", ModuleProjectKind.Contracts, typeof(OrderingModuleMetadata).Assembly),
        new("Ordering", "Ordering.Domain", ModuleProjectKind.Domain, typeof(Order).Assembly),
        new("Ordering", "Ordering.Persistence", ModuleProjectKind.Persistence, typeof(Ordering.Persistence.DependencyInjection).Assembly),

        new("TaskRuntime", "TaskRuntime.Admin.Contracts", ModuleProjectKind.AdminContracts, typeof(TaskRuntimeAdminPermissions).Assembly),
        new("TaskRuntime", "TaskRuntime.AdminCli", ModuleProjectKind.AdminCli, typeof(TaskRuntimeAdminCliModule).Assembly),
        new("TaskRuntime", "TaskRuntime.AdminApi", ModuleProjectKind.AdminApi, typeof(TaskRuntimeAdminApiModule).Assembly),
        new("TaskRuntime", "TaskRuntime.Application", ModuleProjectKind.Application, typeof(TaskRuntime.Application.DependencyInjection).Assembly),
        new("TaskRuntime", "TaskRuntime.Contracts", ModuleProjectKind.Contracts, typeof(TaskRuntimeModuleMetadata).Assembly),
        new("TaskRuntime", "TaskRuntime.Persistence", ModuleProjectKind.Persistence, typeof(TaskRuntime.Persistence.DependencyInjection).Assembly),

        new("TaskSamples", "TaskSamples.Application", ModuleProjectKind.Application, typeof(TaskSamples.Application.DependencyInjection).Assembly),
        new("TaskSamples", "TaskSamples.Contracts", ModuleProjectKind.Contracts, typeof(TaskSamplesModuleMetadata).Assembly),

        new("Tenancy", "Tenancy.Api", ModuleProjectKind.Api, typeof(TenancyModule).Assembly),
        new("Tenancy", "Tenancy.Contracts", ModuleProjectKind.Contracts, typeof(TenancyModuleMetadata).Assembly),
    ];

    public static IReadOnlyList<ModuleDescriptor> ModuleDescriptors { get; } =
    [
        AuthModuleMetadata.Descriptor,
        AdministrationModuleMetadata.Descriptor,
        CatalogModuleMetadata.Descriptor,
        OrderingModuleMetadata.Descriptor,
        TaskRuntimeModuleMetadata.Descriptor,
        TaskSamplesModuleMetadata.Descriptor,
        TenancyModuleMetadata.Descriptor,
    ];

    public static IReadOnlyList<string> ModulePrefixes { get; } = ModuleProjects
        .Select(project => project.ModulePrefix)
        .Distinct(StringComparer.Ordinal)
        .ToArray();

    public static IReadOnlyList<Assembly> ModuleBoundaryAssemblies { get; } = ModuleProjects
        .Select(project => project.Assembly)
        .Distinct()
        .ToArray();

    public static IReadOnlyList<Assembly> ApplicationAssemblies { get; } = ModuleProjects
        .Where(project => project.Kind == ModuleProjectKind.Application)
        .Select(project => project.Assembly)
        .Distinct()
        .ToArray();

    public static IReadOnlyList<Assembly> OrderingAssemblies { get; } = ModuleProjects
        .Where(project => string.Equals(project.ModulePrefix, "Ordering", StringComparison.Ordinal))
        .Select(project => project.Assembly)
        .Distinct()
        .ToArray();

    public static IReadOnlyList<Assembly> CommandLineAllowedAssemblies { get; } =
    [
        typeof(AdministrationAdminCliModule).Assembly,
        typeof(AuthAdminCliModule).Assembly,
        typeof(CatalogAdminCliModule).Assembly,
        typeof(TaskRuntimeAdminCliModule).Assembly,
        typeof(Shared.Administration.Cli.AdminCliExecutor).Assembly,
        AdminCliAssemblyReference.Assembly,
    ];

    public static IReadOnlyList<Assembly> CommandLineCheckedAssemblies { get; } = ModuleBoundaryAssemblies
        .Concat(
        [
            typeof(Shared.Administration.Cli.AdminCliExecutor).Assembly,
            typeof(Shared.Administration.Api.AdminApiExecutor).Assembly,
            AdminApiAssemblyReference.Assembly,
        ])
        .Distinct()
        .ToArray();
}

internal sealed record ModuleProject(
    string ModulePrefix,
    string ProjectName,
    ModuleProjectKind Kind,
    Assembly Assembly);

internal enum ModuleProjectKind
{
    AdminCli,
    AdminContracts,
    AdminApi,
    Api,
    Application,
    Contracts,
    Domain,
    Infrastructure,
    Persistence,
}
