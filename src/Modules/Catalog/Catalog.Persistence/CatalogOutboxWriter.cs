namespace Catalog.Persistence;

using Shared.Application.Time;
using Shared.Infrastructure.Messaging;

internal sealed class CatalogOutboxWriter(CatalogDbContext dbContext, ISystemClock clock)
    : EfOutboxWriter<CatalogDbContext>(dbContext, clock, CatalogMigrations.Schema);
