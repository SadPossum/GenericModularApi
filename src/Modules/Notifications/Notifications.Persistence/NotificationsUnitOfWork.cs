namespace Notifications.Persistence;

using Notifications.Contracts;
using Shared.Cqrs.UnitOfWork;

internal sealed class NotificationsUnitOfWork(NotificationsDbContext dbContext) : IUnitOfWork
{
    public string ModuleName => NotificationsModuleMetadata.Name;

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
