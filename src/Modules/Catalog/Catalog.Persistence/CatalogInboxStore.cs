namespace Catalog.Persistence;

using Shared.Application.Identity;
using Shared.Application.Time;
using Shared.Infrastructure.Messaging;

internal sealed class CatalogInboxStore(CatalogDbContext dbContext, ISystemClock clock, IIdGenerator idGenerator)
    : EfInboxStore<CatalogDbContext>(dbContext, clock, idGenerator, CatalogMigrations.Schema);
