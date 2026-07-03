namespace Auth.Persistence;

using Shared.Application.Time;
using Shared.Infrastructure.Messaging;

internal sealed class AuthOutboxWriter(AuthDbContext dbContext, ISystemClock clock)
    : EfOutboxWriter<AuthDbContext>(dbContext, clock, AuthMigrations.Schema);
