namespace Shared.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Shared.Application.Events;
using Shared.Application.Messaging;
using Shared.Application.UnitOfWork;
using Shared.Domain;

public abstract class EfDomainEventUnitOfWork<TDbContext>(
    string moduleName,
    TDbContext dbContext,
    IDomainEventDispatcher domainEventDispatcher) : IUnitOfWork
    where TDbContext : DbContext
{
    public string ModuleName { get; } = IntegrationEventNaming.NormalizeModuleName(moduleName);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        List<IAggregateRoot> aggregatesWithEvents = dbContext.ChangeTracker
            .Entries()
            .Select(entry => entry.Entity)
            .OfType<IAggregateRoot>()
            .Where(aggregate => aggregate.DomainEvents.Count > 0)
            .Distinct()
            .ToList();

        List<IDomainEvent> domainEvents = aggregatesWithEvents
            .SelectMany(aggregate => aggregate.DomainEvents)
            .ToList();

        if (domainEvents.Count > 0)
        {
            await domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken).ConfigureAwait(false);
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        foreach (IAggregateRoot aggregate in aggregatesWithEvents)
        {
            aggregate.ClearDomainEvents();
        }
    }
}
