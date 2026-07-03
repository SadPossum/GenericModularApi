namespace Shared.Tests;

using Shared.Domain;
using Shared.Domain.Models;
using Xunit;

[Trait("Category", "Unit")]
public sealed class DomainEventTests
{
    [Fact]
    public void Aggregate_root_collects_and_clears_domain_events()
    {
        TestAggregate aggregate = new(Guid.NewGuid());
        aggregate.Touch();

        Assert.Single(aggregate.DomainEvents);

        aggregate.ClearDomainEvents();

        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void Aggregate_root_domain_events_are_read_only_to_callers()
    {
        TestAggregate aggregate = new(Guid.NewGuid());
        aggregate.Touch();

        Assert.Throws<NotSupportedException>(() =>
            ((ICollection<IDomainEvent>)aggregate.DomainEvents).Add(
                new TestDomainEvent(Guid.NewGuid(), DateTimeOffset.UtcNow)));
        Assert.Single(aggregate.DomainEvents);
    }

    [Fact]
    public void Aggregate_root_rejects_null_domain_events()
    {
        TestAggregate aggregate = new(Guid.NewGuid());

        Assert.Throws<ArgumentNullException>(aggregate.TouchWithNull);
    }

    private sealed class TestAggregate(Guid id) : AggregateRoot<Guid>(id)
    {
        public void Touch() => this.RaiseDomainEvent(new TestDomainEvent(Guid.NewGuid(), DateTimeOffset.UtcNow));
        public void TouchWithNull() => this.RaiseDomainEvent(null!);
    }

    private sealed record TestDomainEvent(Guid EventId, DateTimeOffset OccurredAtUtc) : IDomainEvent;
}
