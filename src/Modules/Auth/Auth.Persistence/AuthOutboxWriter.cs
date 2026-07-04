namespace Auth.Persistence;

using Microsoft.Extensions.Options;
using Shared.Runtime;
using Shared.Runtime.Time;
using Shared.Messaging.Infrastructure;

internal sealed class AuthOutboxWriter(
    AuthDbContext dbContext,
    ISystemClock clock,
    IOptions<ApplicationIdentityOptions> applicationIdentity)
    : EfOutboxWriter<AuthDbContext>(dbContext, clock, applicationIdentity, AuthMigrations.Schema);
