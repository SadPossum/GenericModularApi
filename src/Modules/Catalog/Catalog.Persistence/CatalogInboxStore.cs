namespace Catalog.Persistence;

using Shared.Runtime.Identity;
using Shared.Runtime.Time;
using Shared.Messaging.Infrastructure;

internal sealed class CatalogInboxStore(CatalogDbContext dbContext, ISystemClock clock, IIdGenerator idGenerator)
    : EfInboxStore<CatalogDbContext>(dbContext, clock, idGenerator, CatalogMigrations.Schema);
