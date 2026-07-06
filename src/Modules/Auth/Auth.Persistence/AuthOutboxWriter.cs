namespace Auth.Persistence;

using Microsoft.Extensions.Options;
using Shared.Messaging;
using Shared.Runtime;
using Shared.Runtime.Time;
using Shared.Messaging.Infrastructure;

internal sealed class AuthOutboxWriter(
    AuthDbContext dbContext,
    ISystemClock clock,
    IOptions<ApplicationIdentityOptions> applicationIdentity,
    IEnumerable<IIntegrationEventScopeResolver> scopeResolvers)
    : EfOutboxWriter<AuthDbContext>(dbContext, clock, applicationIdentity, AuthMigrations.Schema, scopeResolvers);
