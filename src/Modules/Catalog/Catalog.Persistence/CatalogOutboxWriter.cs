namespace Catalog.Persistence;

using Microsoft.Extensions.Options;
using Shared.Runtime;
using Shared.Runtime.Time;
using Shared.Messaging.Infrastructure;

internal sealed class CatalogOutboxWriter(
    CatalogDbContext dbContext,
    ISystemClock clock,
    IOptions<ApplicationIdentityOptions> applicationIdentity)
    : EfOutboxWriter<CatalogDbContext>(dbContext, clock, applicationIdentity, CatalogMigrations.Schema);
