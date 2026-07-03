namespace Auth.Persistence;

using Shared.Application.Events;
using Shared.Infrastructure.Persistence;

internal sealed class AuthUnitOfWork(AuthDbContext dbContext, IDomainEventDispatcher domainEventDispatcher)
    : EfDomainEventUnitOfWork<AuthDbContext>(AuthMigrations.Schema, dbContext, domainEventDispatcher)
{
}
