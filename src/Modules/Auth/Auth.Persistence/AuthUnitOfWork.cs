namespace Auth.Persistence;

using Shared.Application.Events;
using Shared.Persistence.EntityFrameworkCore;

internal sealed class AuthUnitOfWork(AuthDbContext dbContext, IDomainEventDispatcher domainEventDispatcher)
    : EfDomainEventUnitOfWork<AuthDbContext>(AuthMigrations.Schema, dbContext, domainEventDispatcher)
{
}
