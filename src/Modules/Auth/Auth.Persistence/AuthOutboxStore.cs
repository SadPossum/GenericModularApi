namespace Auth.Persistence;

using Microsoft.Extensions.Options;
using Shared.Messaging.Infrastructure;

internal sealed class AuthOutboxStore(AuthDbContext dbContext, IOptions<OutboxOptions> options)
    : EfOutboxStore<AuthDbContext>(dbContext, options, AuthMigrations.Schema);
