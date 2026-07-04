namespace Auth.Persistence;

using Shared.Runtime.Time;
using Shared.Messaging.Infrastructure;

internal sealed class AuthOutboxWriter(AuthDbContext dbContext, ISystemClock clock)
    : EfOutboxWriter<AuthDbContext>(dbContext, clock, AuthMigrations.Schema);
