namespace Auth.Tests;

using Auth.Domain.Aggregates;
using Auth.Domain.Enums;
using Auth.Domain.Events;
using Auth.Domain.ValueObjects;
using Auth.Persistence;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Events;
using Shared.Application.Tenancy;
using Shared.Domain;
using Shared.Infrastructure.Messaging;
using Xunit;

[Trait("Category", "Unit")]
public sealed class AuthUnitOfWorkTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 14, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task SaveChanges_dispatches_domain_events_and_commits_outbox_in_same_save()
    {
        await using AuthDbContext dbContext = CreateDbContext();
        RecordingDomainEventDispatcher dispatcher = new(dbContext);
        AuthUnitOfWork unitOfWork = new(dbContext, dispatcher);
        Member member = CreateMember();

        await dbContext.Members.AddAsync(member);
        await unitOfWork.SaveChangesAsync();

        Assert.Equal(1, dispatcher.DispatchCount);
        Assert.Empty(member.DomainEvents);
        Assert.Equal(1, await dbContext.Members.CountAsync());
        Assert.Equal(1, await dbContext.OutboxMessages.CountAsync());
    }

    [Fact]
    public async Task SaveChanges_keeps_domain_events_when_dispatch_fails()
    {
        await using AuthDbContext dbContext = CreateDbContext();
        ThrowingDomainEventDispatcher dispatcher = new();
        AuthUnitOfWork unitOfWork = new(dbContext, dispatcher);
        Member member = CreateMember();

        await dbContext.Members.AddAsync(member);

        await Assert.ThrowsAsync<InvalidOperationException>(() => unitOfWork.SaveChangesAsync());
        Assert.Single(member.DomainEvents);
    }

    private static AuthDbContext CreateDbContext()
    {
        DbContextOptions<AuthDbContext> options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AuthDbContext(options, new TestTenantContext());
    }

    private static Member CreateMember() =>
        Member.Create(
            new MemberId(Guid.NewGuid()),
            "tenant-a",
            "member@example.com",
            MemberUsernameType.Email,
            "hash",
            new MemberUsernameId(Guid.NewGuid()),
            Guid.NewGuid(),
            Now).Value;

    private sealed class RecordingDomainEventDispatcher(AuthDbContext dbContext) : IDomainEventDispatcher
    {
        public int DispatchCount { get; private set; }

        public Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken)
        {
            foreach (IDomainEvent domainEvent in domainEvents)
            {
                MemberRegisteredDomainEvent memberRegistered = Assert.IsType<MemberRegisteredDomainEvent>(domainEvent);

                dbContext.OutboxMessages.Add(new OutboxMessage(
                    memberRegistered.EventId,
                    "gma.auth.member-registered.v1",
                    typeof(MemberRegisteredDomainEvent).FullName!,
                    1,
                    memberRegistered.TenantId,
                    memberRegistered.OccurredAtUtc,
                    "{}",
                    memberRegistered.OccurredAtUtc));

                this.DispatchCount++;
            }

            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingDomainEventDispatcher : IDomainEventDispatcher
    {
        public Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("Domain event dispatch failed.");
    }

    private sealed class TestTenantContext : ITenantContext
    {
        public bool IsEnabled => false;
        public string? TenantId => "default";
    }
}
