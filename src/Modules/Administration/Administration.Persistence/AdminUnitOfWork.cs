namespace Administration.Persistence;

using Administration.Contracts;
using Shared.Cqrs.UnitOfWork;

internal sealed class AdminUnitOfWork(AdminDbContext dbContext) : IUnitOfWork
{
    public string ModuleName => AdministrationModuleMetadata.Name;

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
