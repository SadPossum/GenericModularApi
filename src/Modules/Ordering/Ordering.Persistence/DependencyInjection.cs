namespace Ordering.Persistence;

using Catalog.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Ordering.Application.Ports;
using Ordering.Persistence.Repositories;
using Shared.Messaging;
using Shared.Cqrs.UnitOfWork;
using Shared.Persistence.EntityFrameworkCore;
using Shared.ProjectionRebuild;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddOrderingPersistence(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddPersistenceOptions(builder.Configuration);

        builder.Services.TryAddModuleDbContext<OrderingDbContext>(options =>
            options.UseConfiguredProvider(
                builder.Configuration,
                OrderingMigrations.SqlServerAssembly,
                OrderingMigrations.PostgreSqlAssembly,
                OrderingMigrations.Schema,
                OrderingMigrations.HistoryTable));

        builder.Services.TryAddScoped<IOrderRepository, OrderRepository>();
        builder.Services.TryAddScoped<IOrderReadRepository, OrderReadRepository>();
        builder.Services.TryAddScoped<ICatalogItemProjectionRepository, CatalogItemProjectionRepository>();
        builder.Services.TryAddScoped<IProjectionRebuildWriter<CatalogItemProjectionExport>, CatalogItemProjectionRebuildWriter>();
        builder.Services.TryAddEnumerable([
            ServiceDescriptor.Scoped<IUnitOfWork, OrderingUnitOfWork>(),
            ServiceDescriptor.Scoped<IInboxStore, OrderingInboxStore>(),
            ServiceDescriptor.Scoped<IOutboxWriter, OrderingOutboxWriter>(),
            ServiceDescriptor.Scoped<IOutboxStore, OrderingOutboxStore>(),
            ServiceDescriptor.Scoped<IProjectionRebuildCheckpointStore, OrderingProjectionRebuildCheckpointStore>(),
            ServiceDescriptor.Scoped<IProjectionRebuildTransactionBoundary, OrderingProjectionRebuildTransactionBoundary>()
        ]);

        return builder;
    }
}
