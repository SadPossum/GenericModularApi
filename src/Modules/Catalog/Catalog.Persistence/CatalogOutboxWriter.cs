namespace Catalog.Persistence;

using Shared.Runtime.Time;
using Shared.Messaging.Infrastructure;

internal sealed class CatalogOutboxWriter(CatalogDbContext dbContext, ISystemClock clock)
    : EfOutboxWriter<CatalogDbContext>(dbContext, clock, CatalogMigrations.Schema);
