namespace Ordering.Persistence;

using Microsoft.Extensions.Options;
using Shared.Messaging;
using Shared.Messaging.Infrastructure;
using Shared.Runtime;
using Shared.Runtime.Time;

internal sealed class OrderingOutboxWriter(
    OrderingDbContext dbContext,
    ISystemClock clock,
    IOptions<ApplicationIdentityOptions> applicationIdentity,
    IEnumerable<IIntegrationEventScopeResolver> scopeResolvers)
    : EfOutboxWriter<OrderingDbContext>(dbContext, clock, applicationIdentity, OrderingMigrations.Schema, scopeResolvers);
