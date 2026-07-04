namespace Catalog.Persistence;

using Shared.Application.Events;
using Shared.Persistence.EntityFrameworkCore;

internal sealed class CatalogUnitOfWork(CatalogDbContext dbContext, IDomainEventDispatcher domainEventDispatcher)
    : EfDomainEventUnitOfWork<CatalogDbContext>(CatalogMigrations.Schema, dbContext, domainEventDispatcher)
{
}
