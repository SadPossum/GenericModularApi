namespace Catalog.Persistence;

using Microsoft.Extensions.Options;
using Shared.Infrastructure.Messaging;

internal sealed class CatalogOutboxStore(CatalogDbContext dbContext, IOptions<OutboxOptions> options)
    : EfOutboxStore<CatalogDbContext>(dbContext, options, CatalogMigrations.Schema);
