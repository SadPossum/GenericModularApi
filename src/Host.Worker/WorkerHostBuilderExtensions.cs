namespace Host.Worker;

using Auth.Contracts;
using Auth.Persistence;
using Catalog.Application;
using Catalog.Contracts;
using Catalog.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ordering.Application;
using Ordering.Contracts;
using Ordering.Persistence;
using ServiceDefaults;
using Shared.Caching.Cqrs;
using Shared.Caching.Redis;
using Shared.Infrastructure;
using Shared.Messaging.Infrastructure;
using Shared.Messaging.Nats.Aspire;
using Shared.ModuleComposition;
using Shared.Tasks.Cqrs;
using Shared.Tasks.Infrastructure;
using Shared.Tenancy.Caching;
using Shared.Tenancy.Messaging.Infrastructure;
using Shared.Tenancy.Tasks;
using TaskRuntime.Application;
using TaskRuntime.Contracts;
using TaskRuntime.Persistence;
using TaskSamples.Application;

public static class WorkerHostBuilderExtensions
{
    public static IHostApplicationBuilder AddWorkerHost(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        WorkerHostOptions workerOptions = WorkerHostOptions.FromConfiguration(builder.Configuration);
        builder.Services.AddSingleton(workerOptions);

        builder.AddRedisCaching();
        builder.AddCachingCqrs();
        builder.AddSharedInfrastructure();
        builder.AddTenantCaching();
        builder.AddMessagingInfrastructure();
        builder.AddTenantAwareMessaging();
        builder.AddConfiguredNatsJetStreamMessaging();

        if (workerOptions.NatsConsumersEnabled)
        {
            builder.AddConfiguredNatsJetStreamConsumers();
        }

        AddConfiguredModuleGroups(builder, workerOptions);

        if (workerOptions.TaskWorkerEnabled)
        {
            builder.AddTenantTaskExecutionContext();
            builder.AddTaskCqrs();
            builder.AddTaskWorkerRuntime();
        }

        builder.AddServiceDefaults();
        return builder;
    }

    public static IHost LogWorkerStartupSummary(this IHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        WorkerHostOptions workerOptions = host.Services.GetRequiredService<WorkerHostOptions>();
        ILogger logger = host.Services
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("Host.Worker");
        string moduleList = workerOptions.GetComposedModuleNames() is { Count: > 0 } modules
            ? string.Join(", ", modules)
            : "none";

        logger.LogInformation(
            "Host.Worker starting. NATS publishing enabled: {NatsPublishingEnabled}; NATS consumers enabled: {NatsConsumersEnabled}; task workers enabled: {TaskWorkerEnabled}; composed modules: {WorkerModules}.",
            workerOptions.NatsPublishingEnabled,
            workerOptions.NatsConsumersEnabled,
            workerOptions.TaskWorkerEnabled,
            moduleList);

        return host;
    }

    private static void AddConfiguredModuleGroups(IHostApplicationBuilder builder, WorkerHostOptions workerOptions)
    {
        if (workerOptions.Modules.Auth)
        {
            builder.SelectModuleProfile(AuthProfile.TenantScoped().Descriptor, "Host.Worker/Auth");
            builder.AddAuthPersistence();
        }

        if (workerOptions.Modules.Catalog)
        {
            builder.SelectModuleProfile(CatalogProfiles.Default, "Host.Worker/Catalog");
            builder.Services.AddCatalogApplication();
            builder.AddCatalogPersistence();
        }

        if (workerOptions.Modules.Ordering)
        {
            builder.SelectModuleProfile(OrderingProfiles.Default, "Host.Worker/Ordering");
            builder.Services.AddOrderingApplication();
            builder.AddOrderingPersistence();
        }

        if (workerOptions.Modules.TaskRuntime)
        {
            builder.SelectModuleProfile(TaskRuntimeProfiles.Default, "Host.Worker/TaskRuntime");
            builder.Services.AddTaskRuntimeApplication();
            builder.AddTaskRuntimePersistence();
        }

        if (workerOptions.Modules.TaskSamples)
        {
            builder.AddTaskCqrs();
            builder.Services.AddTaskSamplesApplication();
        }
    }
}
